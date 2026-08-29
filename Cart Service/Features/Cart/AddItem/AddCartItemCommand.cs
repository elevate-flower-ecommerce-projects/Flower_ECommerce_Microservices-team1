using Flower.Common.StandardizedResponse;
using MediatR;

namespace Cart_Service.Features.Cart.AddItem;

public sealed record AddCartItemCommand(
    string UserId,
    Guid ProductId,
    int Quantity,
    Guid? StoreId) : IRequest<OperationResult<object>>;
