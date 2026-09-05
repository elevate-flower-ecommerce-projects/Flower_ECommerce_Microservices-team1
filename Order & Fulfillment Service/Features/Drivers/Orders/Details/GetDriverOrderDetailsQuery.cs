using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Drivers;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.Details;

public sealed record GetDriverOrderDetailsQuery(Guid OrderId, string DriverUserId)
    : IRequest<OperationResult<DriverOrderDetailsResponse>>;
