using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Drivers;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.List;

public sealed class GetMyDriverOrdersHandler(IUnitOfWork<OrderDbContext> unitOfWork)
    : IRequestHandler<GetMyDriverOrdersQuery, OperationResult<IReadOnlyList<DriverOrderSummaryResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<DriverOrderSummaryResponse>>> Handle(
        GetMyDriverOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await unitOfWork.Repository<Entities.Order, Guid>()
            .Query()
            .Include(order => order.Items)
            .Where(order => order.AssignedDriverUserId == request.DriverUserId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<DriverOrderSummaryResponse>>(
            orders.Select(order => order.ToSummary()).ToList(),
            "Assigned orders retrieved successfully.",
            "Assigned orders retrieved successfully.");
    }
}
