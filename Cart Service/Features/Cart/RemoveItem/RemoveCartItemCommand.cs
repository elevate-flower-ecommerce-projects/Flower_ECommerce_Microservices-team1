using Cart_Service.Contracts.Cart;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Cart_Service.Features.Cart.RemoveItem;

public sealed record RemoveCartItemCommand(
    string UserId,
    Guid ItemId,
    Guid? StoreId) : IRequest<OperationResult<CartResponse>>;
