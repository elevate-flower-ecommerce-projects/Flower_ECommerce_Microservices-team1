using Cart_Service.Contracts.Cart;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Cart_Service.Features.Cart.GetCart;

public sealed record GetCartQuery(string UserId, Guid? StoreId) : IRequest<OperationResult<CartResponse>>;
