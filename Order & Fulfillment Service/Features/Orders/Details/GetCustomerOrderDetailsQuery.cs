using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Orders;

namespace Order___Fulfillment_Service.Features.Orders.Details;

public sealed record GetCustomerOrderDetailsQuery(
    string UserId,
    Guid OrderId) : IRequest<OperationResult<OrderDetailResponse>>;