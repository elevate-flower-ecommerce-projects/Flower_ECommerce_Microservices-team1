using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Drivers;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.Details;

public sealed class GetDriverOrderDetailsHandler(IUnitOfWork<OrderDbContext> unitOfWork)
    : IRequestHandler<GetDriverOrderDetailsQuery, OperationResult<DriverOrderDetailsResponse>>
{
    public async Task<OperationResult<DriverOrderDetailsResponse>> Handle(
        GetDriverOrderDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<Entities.Order, Guid>()
            .Query()
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.OrderId, cancellationToken);

        if (order is null)
            return OperationResultFactory.NotFound<DriverOrderDetailsResponse>(message: OrderMessages.OrderNotFound, messageLocalized: OrderMessages.OrderNotFound);
        if (!string.Equals(order.AssignedDriverUserId, request.DriverUserId, StringComparison.Ordinal))
            return OperationResultFactory.Forbidden<DriverOrderDetailsResponse>(message: OrderMessages.OrderNotAssigned, messageLocalized: OrderMessages.OrderNotAssigned);

        return OperationResultFactory.Success(order.ToDriverDetails(), "Driver order details retrieved successfully.", "Driver order details retrieved successfully.");
    }
}
