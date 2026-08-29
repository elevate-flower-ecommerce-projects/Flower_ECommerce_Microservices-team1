using Flower.Common.StandardizedResponse;
using MediatR;

namespace Cart_Service.Features.Cart.UpdateQuantity;

public sealed record UpdateCartItemQuantityCommand(
    string UserId,
    Guid ItemId,
    int Quantity,
    Guid? StoreId) : IRequest<OperationResult<object>>;
