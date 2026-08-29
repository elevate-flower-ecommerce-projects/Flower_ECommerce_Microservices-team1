using Cart_Service.Contracts.Cart;
using Cart_Service.Entities;
using Cart_Service.Infrastructure.Catalog;
using Cart_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Repository.Layer.Interfaces;

namespace Cart_Service.Features.Cart.UpdateQuantity;

public sealed class UpdateCartItemQuantityHandler(
    IUnitOfWork<CartDbContext> unitOfWork,
    ICatalogClient catalogClient,
    ICartResponseBuilder responseBuilder)
    : IRequestHandler<UpdateCartItemQuantityCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        UpdateCartItemQuantityCommand request,
        CancellationToken cancellationToken)
    {
        // The quantity is absolute rather than a delta, so repeated calls from a fast stepper
        // converge on the same result instead of accumulating.
        var errors = CartInputValidator.Validate(
            productId: null,
            request.Quantity,
            request.StoreId,
            allowZero: true);

        if (errors.Count > 0)
        {
            return OperationResultFactory.Validation<object>(
                errors,
                CartMessages.ValidationFailed,
                CartMessages.ValidationFailed);
        }

        var cart = await unitOfWork.FindCartWithItemsAsync(request.UserId, cancellationToken);
        var item = cart?.Items.SingleOrDefault(candidate => candidate.Id == request.ItemId);

        if (cart is null || item is null)
        {
            return OperationResultFactory.NotFound<object>(
                message: CartMessages.ItemNotFound,
                messageLocalized: CartMessages.ItemNotFound);
        }

        var now = DateTime.UtcNow;

        // Stepping down to zero is the documented way to remove a line.
        if (request.Quantity == 0)
        {
            await unitOfWork.Repository<CartItem, Guid>().Delete(item);
            cart.UpdatedAtUtc = now;
            await unitOfWork.CompleteAsync();
            cart.Items.Remove(item);

            var afterRemoval = await responseBuilder.BuildAsync(cart, request.StoreId, cancellationToken);
            return OperationResultFactory.Success<object>(
                afterRemoval,
                CartMessages.ItemRemoved,
                CartMessages.ItemRemoved);
        }

        var lookup = await catalogClient.GetProductAsync(item.ProductId, request.StoreId, cancellationToken);

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
                new StockLimitResponse(item.ProductId, request.Quantity, product.AvailableQuantity),
                CartMessages.OutOfStock,
                CartMessages.OutOfStock);
        }

        if (request.Quantity > product.AvailableQuantity)
        {
            return OperationResultFactory.Conflict<object>(
                new StockLimitResponse(item.ProductId, request.Quantity, product.AvailableQuantity),
                CartMessages.QuantityExceedsStock,
                CartMessages.QuantityExceedsStock);
        }

        item.Quantity = request.Quantity;
        item.UnitPriceSnapshot = product.EffectivePrice;
        item.ProductName = product.Name;
        item.ImageUrl = product.ImageUrls.FirstOrDefault();
        item.UpdatedAtUtc = now;
        cart.UpdatedAtUtc = now;

        await unitOfWork.Repository<CartItem, Guid>().Update(item);
        await unitOfWork.CompleteAsync();

        var response = await responseBuilder.BuildAsync(cart, request.StoreId, cancellationToken);

        return OperationResultFactory.Success<object>(
            response,
            CartMessages.QuantityUpdated,
            CartMessages.QuantityUpdated);
    }
}
