using System.Security.Cryptography;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Checkout;
using Order___Fulfillment_Service.Entities;
using Order___Fulfillment_Service.Infrastructure.Clients;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Order___Fulfillment_Service.Features.Checkout.PlaceOrder;

public sealed class PlaceOrderHandler(
    IUnitOfWork<OrderDbContext> unitOfWork,
    ICheckoutQuoteBuilder quoteBuilder,
    ICartClient cartClient,
    ILogger<PlaceOrderHandler> logger)
    : IRequestHandler<PlaceOrderCommand, OperationResult<object>>
{
    private const int ThumbnailUrlMaxLength = 500;

    public async Task<OperationResult<object>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        if (!CheckoutValidator.IsValidIdempotencyKey(command.IdempotencyKey))
        {
            return OperationResultFactory.BadRequest<object>(
                message: CheckoutMessages.IdempotencyKeyRequired,
                messageLocalized: CheckoutMessages.IdempotencyKeyRequired);
        }

        var request = command.Request;
        var errors = CheckoutValidator.ValidatePlaceOrder(request);
        if (errors.Count > 0)
            return OperationResultFactory.Validation<object>(errors, CheckoutMessages.ValidationFailed, CheckoutMessages.ValidationFailed);

        var idempotencyKey = command.IdempotencyKey!.Trim();

        // A retry of an attempt that already created an order gets that order back. This runs before
        // any other check because a Cash on Delivery order has already emptied the cart.
        var existing = await FindByIdempotencyKeyAsync(command.CustomerUserId, idempotencyKey, cancellationToken);
        if (existing is not null)
            return AlreadyPlaced(existing);

        var result = await quoteBuilder.BuildAsync(request.AddressId, request.Gift, cancellationToken);
        if (result.Failure is not null)
            return result.Failure;

        var quote = result.Quote!;

        // The client total is only compared, never charged: a mismatch means the customer agreed to a
        // different amount, so they must see the new one first.
        if (request.ExpectedTotal != quote.Total)
        {
            return CheckoutQuoteBuilder.Conflict(
                CheckoutErrorCodes.PriceChanged,
                CheckoutMessages.PriceChanged,
                summary: quote.ToSummary());
        }

        var paymentMethod = request.PaymentMethod!.Value;
        var order = CreateOrder(command.CustomerUserId, idempotencyKey, paymentMethod, request.Gift, quote);

        await unitOfWork.Repository<Order, Guid>().Create(order);
        try
        {
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException)
        {
            // Two retries with the same key raced past the lookup; the unique index let one win.
            var winner = await FindByIdempotencyKeyAsync(command.CustomerUserId, idempotencyKey, cancellationToken);
            if (winner is null)
                throw;

            return AlreadyPlaced(winner);
        }

        // The order is saved first because it must not be lost; clearing the cart can be repeated.
        // A card order keeps the cart until payment succeeds.
        if (paymentMethod is PaymentMethodType.CashOnDelivery
            && !await cartClient.ClearCartAsync(cancellationToken))
        {
            logger.LogWarning("Order {OrderNumber} was placed but the cart could not be cleared.", order.OrderNumber);
        }

        return OperationResultFactory.Created<object>(ToResponse(order), CheckoutMessages.OrderPlaced, CheckoutMessages.OrderPlaced);
    }

    private Task<Order?> FindByIdempotencyKeyAsync(string customerUserId, string idempotencyKey, CancellationToken cancellationToken)
        => unitOfWork.Repository<Order, Guid>()
            .Query()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                order => order.CustomerUserId == customerUserId && order.IdempotencyKey == idempotencyKey,
                cancellationToken);

    private static OperationResult<object> AlreadyPlaced(Order order)
        => OperationResultFactory.Created<object>(ToResponse(order), CheckoutMessages.OrderAlreadyPlaced, CheckoutMessages.OrderAlreadyPlaced);

    private static Order CreateOrder(
        string customerUserId,
        string idempotencyKey,
        PaymentMethodType paymentMethod,
        GiftDetailsRequest? gift,
        CheckoutQuote quote)
    {
        var address = quote.DeliveryAddress;
        var giftMessage = gift?.Message?.Trim();

        return new Order
        {
            OrderNumber = $"FL-{DateTime.UtcNow:yyMMdd}-{RandomNumberGenerator.GetInt32(0, 1_000_000):D6}",
            CustomerUserId = customerUserId,
            IdempotencyKey = idempotencyKey,
            Status = paymentMethod is PaymentMethodType.Card ? OrderStatus.PendingPayment : OrderStatus.Placed,
            PaymentMethod = paymentMethod,
            PaymentStatus = PaymentStatus.Pending,

            // The store resolve endpoint returns only the id; name, address and location stay empty
            // until the Address service exposes them to non-admin callers.
            StoreId = quote.StoreId,

            RecipientName = address.RecipientName,
            RecipientPhone = address.Phone,
            DeliveryAddressLine = address.AddressLine,
            DeliveryCity = address.City,
            DeliveryArea = address.Area,
            DeliveryLatitude = quote.DeliveryLatitude ?? 0m,
            DeliveryLongitude = quote.DeliveryLongitude ?? 0m,
            IsGift = address.IsGift,
            GiftMessage = string.IsNullOrWhiteSpace(giftMessage) ? null : giftMessage,

            Subtotal = quote.Subtotal,
            DeliveryFee = quote.DeliveryFee,
            Discount = quote.Discount,
            Total = quote.Total,

            Items = quote.Items
                .Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Name,
                    ThumbnailUrl = item.ImageUrl?.Length > ThumbnailUrlMaxLength ? null : item.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineSubtotal
                })
                .ToList()
        };
    }

    private static PlacedOrderResponse ToResponse(Order order)
        => new(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.PaymentMethod,
            order.PaymentStatus,
            PaymentRequired: order.PaymentMethod is PaymentMethodType.Card && order.PaymentStatus is not PaymentStatus.Paid,
            order.Subtotal,
            order.DeliveryFee,
            order.Discount,
            order.Total);
}
