using Flower.Common.StandardizedResponse;
using MediatR;

namespace Cart_Service.Features.Cart.ClearCart;

public sealed record ClearCartCommand(string UserId) : IRequest<OperationResult>;
