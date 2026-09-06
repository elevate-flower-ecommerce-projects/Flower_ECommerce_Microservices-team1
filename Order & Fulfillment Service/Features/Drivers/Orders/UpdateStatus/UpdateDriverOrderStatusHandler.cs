using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Drivers;
using Order___Fulfillment_Service.Entities;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.UpdateStatus;

public sealed class UpdateDriverOrderStatusHandler(IUnitOfWork<OrderDbContext> unitOfWork)
    : IRequestHandler<UpdateDriverOrderStatusCommand, OperationResult<DriverOrderDetailsResponse>>
{
    public async Task<OperationResult<DriverOrderDetailsResponse>> Handle(
        UpdateDriverOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
        {
            return OperationResultFactory.Error<DriverOrderDetailsResponse>(
                message: "A valid order status is required.",
                messageLocalized: "A valid order status is required.",
                statusCode: StatusCode.ValidationError);
        }

        var orderRepository = unitOfWork.Repository<Order, Guid>();
        var order = await orderRepository
            .Query(false)
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.OrderId, cancellationToken);

        if (order is null)
            return OperationResultFactory.NotFound<DriverOrderDetailsResponse>(message: OrderMessages.OrderNotFound, messageLocalized: OrderMessages.OrderNotFound);
        if (!string.Equals(order.AssignedDriverUserId, request.DriverUserId, StringComparison.Ordinal))
            return OperationResultFactory.Forbidden<DriverOrderDetailsResponse>(message: OrderMessages.OrderNotAssigned, messageLocalized: OrderMessages.OrderNotAssigned);
        if (!OrderStatusTransitions.IsValidDriverTransition(order.Status, request.Status))
            return OperationResultFactory.Conflict<DriverOrderDetailsResponse>(order.ToDriverDetails(), OrderMessages.InvalidStatusTransition, OrderMessages.InvalidStatusTransition);

        order.Status = request.Status;
        if (request.Status == OrderStatus.Delivered)
            order.DeliveredAtUtc = DateTime.UtcNow;

        await orderRepository.Update(order);
        await unitOfWork.CompleteAsync();

        return OperationResultFactory.Success(order.ToDriverDetails(), "Order status updated successfully.", "Order status updated successfully.");
    }
}
