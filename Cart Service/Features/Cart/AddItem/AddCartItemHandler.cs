using Cart_Service.Contracts.Cart;
using Cart_Service.Entities;
using Cart_Service.Infrastructure.Catalog;
using Cart_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Repository.Layer.Interfaces;

namespace Cart_Service.Features.Cart.AddItem;

public sealed class AddCartItemHandler(
    IUnitOfWork<CartDbContext> unitOfWork,
    ICatalogClient catalogClient,
    ICartResponseBuilder responseBuilder)
    : IRequestHandler<AddCartItemCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        AddCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var errors = CartInputValidator.Validate(
            request.ProductId,
            request.Quantity,
            request.StoreId,
            allowZero: false);

        if (errors.Count > 0)
        {
            return OperationResultFactory.Validation<object>(
                errors,
                CartMessages.ValidationFailed,
                CartMessages.ValidationFailed);
        }

        var lookup = await catalogClient.GetProductAsync(request.ProductId, request.StoreId, cancellationToken);

        switch (lookup.Status)
        {
            case CatalogLookupStatus.NotFound:
                return OperationResultFactory.NotFound<object>(
                    message: CartMessages.ProductNotFound,
                    messageLocalized: CartMessages.ProductNotFound);

            case CatalogLookupStatus.Unavailable:
                return OperationResultFactory.Error<object>(
                    message: CartMessages.CatalogUnavailable,
                    messageLocalized: CartMessages.CatalogUnavailable,
                    statusCode: StatusCode.ServiceUnavailable);
        }

        var product = lookup.Product!;

        if (!product.InStock)
        {
            return OperationResultFactory.Conflict<object>(
                new StockLimitResponse(request.ProductId, request.Quantity, product.AvailableQuantity),
                CartMessages.OutOfStock,
                CartMessages.OutOfStock);
        }

        var cartRepository = unitOfWork.Repository<Entities.Cart, Guid>();
        var cart = await unitOfWork.FindCartWithItemsAsync(request.UserId, cancellationToken);
        var isNewCart = cart is null;

        cart ??= new Entities.Cart
        {
            Id = Guid.CreateVersion7(),
            UserId = request.UserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        var existingItem = cart.Items.SingleOrDefault(item => item.ProductId == request.ProductId);

        // Adding a product already in the cart increases the existing line instead of creating a
        // second one, and the stock check applies to the resulting total.
        var resultingQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (resultingQuantity > product.AvailableQuantity)
        {
            return OperationResultFactory.Conflict<object>(
                new StockLimitResponse(request.ProductId, resultingQuantity, product.AvailableQuantity),
                CartMessages.QuantityExceedsStock,
                CartMessages.QuantityExceedsStock);
        }

        if (resultingQuantity > CartInputValidator.MaxQuantityPerLine)
        {
            return OperationResultFactory.Validation<object>(
                new Dictionary<string, string[]>
                {
                    ["quantity"] = [$"Quantity must not exceed {CartInputValidator.MaxQuantityPerLine}."]
                },
                CartMessages.ValidationFailed,
                CartMessages.ValidationFailed);
        }

        var now = DateTime.UtcNow;

        if (existingItem is null)
        {
            var newItem = new CartItem
            {
                Id = Guid.CreateVersion7(),
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = resultingQuantity,
                ProductName = product.Name,
                ImageUrl = product.ImageUrls.FirstOrDefault(),
                UnitPriceSnapshot = product.EffectivePrice,
                AddedAtUtc = now
            };

            cart.Items.Add(newItem);

            // An existing cart is already tracked, so only the new line needs to be inserted.
            // Marking the whole cart graph as modified would try to UPDATE the new row instead.
            if (!isNewCart)
            {
                await unitOfWork.Repository<CartItem, Guid>().Create(newItem);
            }
        }
        else
        {
            existingItem.Quantity = resultingQuantity;
            existingItem.UnitPriceSnapshot = product.EffectivePrice;
            existingItem.ProductName = product.Name;
            existingItem.ImageUrl = product.ImageUrls.FirstOrDefault();
            existingItem.UpdatedAtUtc = now;
        }

        cart.UpdatedAtUtc = now;

        if (isNewCart)
        {
            await cartRepository.Create(cart);
        }

        await unitOfWork.CompleteAsync();

        var response = await responseBuilder.BuildAsync(cart, request.StoreId, cancellationToken);

        return existingItem is null
            ? OperationResultFactory.Created<object>(response, CartMessages.ItemAdded, CartMessages.ItemAdded)
            : OperationResultFactory.Success<object>(response, CartMessages.QuantityUpdated, CartMessages.QuantityUpdated);
    }
}
