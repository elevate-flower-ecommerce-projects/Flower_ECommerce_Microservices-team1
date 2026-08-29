using Cart_Service.Contracts.Cart;
using Cart_Service.Entities;
using Cart_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Repository.Layer.Interfaces;

namespace Cart_Service.Features.Cart.RemoveItem;

public sealed class RemoveCartItemHandler(
    IUnitOfWork<CartDbContext> unitOfWork,
    ICartResponseBuilder responseBuilder)
    : IRequestHandler<RemoveCartItemCommand, OperationResult<CartResponse>>
{
    public async Task<OperationResult<CartResponse>> Handle(
        RemoveCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await unitOfWork.FindCartWithItemsAsync(request.UserId, cancellationToken);
        var item = cart?.Items.SingleOrDefault(candidate => candidate.Id == request.ItemId);

        if (cart is null || item is null)
        {
            return OperationResultFactory.NotFound<CartResponse>(
                message: CartMessages.ItemNotFound,
                messageLocalized: CartMessages.ItemNotFound);
        }

        await unitOfWork.Repository<CartItem, Guid>().Delete(item);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await unitOfWork.CompleteAsync();
        cart.Items.Remove(item);

        var response = await responseBuilder.BuildAsync(cart, request.StoreId, cancellationToken);

        return OperationResultFactory.Success(
            response,
            CartMessages.ItemRemoved,
            CartMessages.ItemRemoved);
    }
}
