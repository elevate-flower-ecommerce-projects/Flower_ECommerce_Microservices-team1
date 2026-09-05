using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Drivers;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.List;

public sealed record GetMyDriverOrdersQuery(string DriverUserId)
    : IRequest<OperationResult<IReadOnlyList<DriverOrderSummaryResponse>>>;
