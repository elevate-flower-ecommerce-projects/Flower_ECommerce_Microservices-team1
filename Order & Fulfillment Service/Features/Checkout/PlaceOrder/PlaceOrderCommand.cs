using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Checkout;

namespace Order___Fulfillment_Service.Features.Checkout.PlaceOrder;

public sealed record PlaceOrderCommand(
    string CustomerUserId,
    string? IdempotencyKey,
    PlaceOrderRequest Request) : IRequest<OperationResult<object>>;
