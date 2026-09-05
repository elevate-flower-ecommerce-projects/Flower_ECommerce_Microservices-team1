using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Entities;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Order___Fulfillment_Service.Features.Drivers.Location;

public sealed class ReportDriverLocationHandler(IUnitOfWork<OrderDbContext> unitOfWork)
    : IRequestHandler<ReportDriverLocationCommand, OperationResult>
{
    public async Task<OperationResult> Handle(ReportDriverLocationCommand request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.Latitude is < -90 or > 90)
            errors["latitude"] = ["Latitude must be between -90 and 90."];
        if (request.Longitude is < -180 or > 180)
            errors["longitude"] = ["Longitude must be between -180 and 180."];
        if (errors.Count > 0)
            return OperationResultFactory.Validation(errors, OrderMessages.ValidationFailed, OrderMessages.ValidationFailed);

        var order = await unitOfWork.Repository<Order, Guid>()
            .Query()
            .SingleOrDefaultAsync(candidate => candidate.Id == request.OrderId, cancellationToken);

        if (order is null)
            return OperationResultFactory.NotFound(message: OrderMessages.OrderNotFound, messageLocalized: OrderMessages.OrderNotFound);
        if (!string.Equals(order.AssignedDriverUserId, request.DriverUserId, StringComparison.Ordinal))
            return OperationResultFactory.Forbidden(message: OrderMessages.OrderNotAssigned, messageLocalized: OrderMessages.OrderNotAssigned);
        if (!OrderStatusTransitions.IsActiveDelivery(order.Status))
            return OperationResultFactory.Conflict(OrderMessages.InactiveDelivery, OrderMessages.InactiveDelivery);

        await unitOfWork.Repository<DriverLocation, Guid>().Create(new DriverLocation
        {
            OrderId = order.Id,
            DriverUserId = request.DriverUserId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RecordedAtUtc = DateTime.UtcNow
        });
        await unitOfWork.CompleteAsync();

        return OperationResultFactory.NoContent("Driver location recorded successfully.", "Driver location recorded successfully.");
    }
}
