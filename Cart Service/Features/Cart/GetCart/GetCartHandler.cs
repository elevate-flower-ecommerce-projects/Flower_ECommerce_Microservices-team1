using Cart_Service.Contracts.Cart;
using Cart_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Repository.Layer.Interfaces;

namespace Cart_Service.Features.Cart.GetCart;

public sealed class GetCartHandler(
    IUnitOfWork<CartDbContext> unitOfWork,
    ICartResponseBuilder responseBuilder)
    : IRequestHandler<GetCartQuery, OperationResult<CartResponse>>
{
    public async Task<OperationResult<CartResponse>> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        var cart = await unitOfWork.FindCartWithItemsAsync(request.UserId, cancellationToken);

        // A customer who never added anything has an empty cart, not a missing resource.
        var response = await responseBuilder.BuildAsync(cart, request.StoreId, cancellationToken);

        return OperationResultFactory.Success(
            response,
            response.IsEmpty ? CartMessages.CartEmpty : CartMessages.CartLoaded,
            response.IsEmpty ? CartMessages.CartEmpty : CartMessages.CartLoaded);
    }
}
