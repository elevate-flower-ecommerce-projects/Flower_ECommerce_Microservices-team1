using Flower.Common.StandardizedResponse;
using Microsoft.Extensions.Options;
using Order___Fulfillment_Service.Contracts.Checkout;
using Order___Fulfillment_Service.Infrastructure.Clients;
using Order___Fulfillment_Service.Settings;

namespace Order___Fulfillment_Service.Features.Checkout;

/// <summary>Everything an order needs, computed on the server from the Address and Cart services.</summary>
public sealed record CheckoutQuote(
    CheckoutDeliveryAddressResponse DeliveryAddress,
    decimal? DeliveryLatitude,
    decimal? DeliveryLongitude,
    Guid StoreId,
    IReadOnlyList<CheckoutItemResponse> Items,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total)
{
    public CheckoutSummaryResponse ToSummary()
        => new(DeliveryAddress, Items, Subtotal, DeliveryFee, Discount, Total);
}

/// <summary>Either a quote or the response that explains why there is none.</summary>
public sealed record CheckoutQuoteResult(CheckoutQuote? Quote, OperationResult<object>? Failure);

public interface ICheckoutQuoteBuilder
{
    /// <summary>Call only with a request that passed <see cref="CheckoutValidator.ValidateDelivery"/>.</summary>
    Task<CheckoutQuoteResult> BuildAsync(Guid? addressId, GiftDetailsRequest? gift, CancellationToken cancellationToken);
}

public sealed class CheckoutQuoteBuilder(
    IAddressClient addressClient,
    ICartClient cartClient,
    IOptions<CheckoutOptions> options) : ICheckoutQuoteBuilder
{
    public async Task<CheckoutQuoteResult> BuildAsync(
        Guid? addressId,
        GiftDetailsRequest? gift,
        CancellationToken cancellationToken)
    {
        // 1. Where the order goes: a gift address, the chosen saved address, or the default one.
        var delivery = await ResolveDeliveryAsync(addressId, gift, cancellationToken);
        if (delivery.Failure is not null)
            return new CheckoutQuoteResult(null, delivery.Failure);

        var (address, latitude, longitude) = delivery.Value!;

        // 2. Which store serves it. Resolved again at checkout rather than trusting the store saved
        //    with the address, because coverage can change after the address was created.
        var resolution = await addressClient.ResolveStoreAsync(address.City, address.Area, latitude, longitude, cancellationToken);
        if (resolution.Status is not DownstreamStatus.Found)
            return Fail(Unavailable());
        if (!resolution.Value!.IsServiceable || resolution.Value.StoreId is not { } storeId)
            return Fail(Conflict(CheckoutErrorCodes.AddressNotServiceable, CheckoutMessages.AddressNotServiceable));

        // 3. The cart priced and stock-checked at that store.
        var cart = await cartClient.GetCartAsync(storeId, cancellationToken);
        if (cart.Status is not DownstreamStatus.Found || cart.Value!.PricingUnavailable)
            return Fail(Unavailable());
        if (cart.Value.IsEmpty || cart.Value.Lines.Count == 0)
            return Fail(Conflict(CheckoutErrorCodes.CartEmpty, CheckoutMessages.CartEmpty));

        var unavailable = cart.Value.Lines
            .Where(line => line.OutOfStock || !line.InStock || line.AvailableQuantity < line.Quantity)
            .Select(line => new UnavailableCheckoutItemResponse(
                line.ProductId,
                line.Name,
                line.Quantity,
                line.OutOfStock || !line.InStock ? 0 : line.AvailableQuantity))
            .ToList();
        if (unavailable.Count > 0)
            return Fail(Conflict(CheckoutErrorCodes.ItemsUnavailable, CheckoutMessages.ItemsUnavailable, items: unavailable));

        // 4. The money, always computed here and never taken from the client.
        var items = cart.Value.Lines
            .Select(line => new CheckoutItemResponse(
                line.ProductId,
                line.Name,
                line.ImageUrl,
                line.UnitPrice,
                line.Quantity,
                line.UnitPrice * line.Quantity))
            .ToList();

        var subtotal = items.Sum(item => item.LineSubtotal);
        var deliveryFee = options.Value.DeliveryFee;
        const decimal discount = 0m;
        var total = subtotal + deliveryFee - discount;

        return new CheckoutQuoteResult(
            new CheckoutQuote(address, latitude, longitude, storeId, items, subtotal, deliveryFee, discount, total),
            null);
    }

    private async Task<(DeliveryTarget? Value, OperationResult<object>? Failure)> ResolveDeliveryAsync(
        Guid? addressId,
        GiftDetailsRequest? gift,
        CancellationToken cancellationToken)
    {
        if (gift is not null)
        {
            var giftAddress = new CheckoutDeliveryAddressResponse(
                null,
                IsGift: true,
                gift.RecipientName!.Trim(),
                gift.Phone!.Trim(),
                gift.AddressLine!.Trim(),
                gift.City!.Trim(),
                gift.Area!.Trim());

            return (new DeliveryTarget(giftAddress, gift.Lat, gift.Lng), null);
        }

        var chosenId = addressId;
        if (chosenId is null)
        {
            var preferred = await addressClient.GetPreferredAddressIdAsync(cancellationToken);
            if (preferred.Status is DownstreamStatus.Unavailable)
                return (null, Unavailable());
            if (preferred.Status is DownstreamStatus.NotFound)
                return (null, Conflict(CheckoutErrorCodes.AddressRequired, CheckoutMessages.AddressRequired));

            chosenId = preferred.Value;
        }

        var saved = await addressClient.GetAddressAsync(chosenId.Value, cancellationToken);
        if (saved.Status is DownstreamStatus.Unavailable)
            return (null, Unavailable());
        if (saved.Status is DownstreamStatus.NotFound)
        {
            return (null, OperationResultFactory.NotFound<object>(
                message: CheckoutMessages.AddressNotFound,
                messageLocalized: CheckoutMessages.AddressNotFound));
        }

        var address = saved.Value!;
        return (new DeliveryTarget(
            new CheckoutDeliveryAddressResponse(
                address.Id,
                IsGift: false,
                address.RecipientName,
                address.Phone,
                address.AddressLine,
                address.City,
                address.Area),
            address.Lat,
            address.Lng), null);
    }

    private static CheckoutQuoteResult Fail(OperationResult<object> failure) => new(null, failure);

    internal static OperationResult<object> Conflict(
        string code,
        string message,
        IReadOnlyList<UnavailableCheckoutItemResponse>? items = null,
        CheckoutSummaryResponse? summary = null)
        => OperationResultFactory.Conflict<object>(new CheckoutErrorResponse(code, items, summary), message, message);

    private static OperationResult<object> Unavailable()
        => OperationResultFactory.Error<object>(
            new CheckoutErrorResponse(CheckoutErrorCodes.DependencyUnavailable),
            CheckoutMessages.DependencyUnavailable,
            CheckoutMessages.DependencyUnavailable,
            StatusCode.ServiceUnavailable);

    private sealed record DeliveryTarget(CheckoutDeliveryAddressResponse Address, decimal? Latitude, decimal? Longitude);
}
