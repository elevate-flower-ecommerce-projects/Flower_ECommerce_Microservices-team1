using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Drivers;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.UpdateStatus;

public sealed record UpdateDriverOrderStatusCommand(Guid OrderId, string DriverUserId, OrderStatus Status)
    : IRequest<OperationResult<DriverOrderDetailsResponse>>;
