using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Orders;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;

namespace Order___Fulfillment_Service.Features.Orders.Details;

public sealed class GetCustomerOrderDetailsHandler(OrderDbContext dbContext)
    : IRequestHandler<GetCustomerOrderDetailsQuery, OperationResult<OrderDetailResponse>>
{
    public async Task<OperationResult<OrderDetailResponse>> Handle(
        GetCustomerOrderDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(candidate => candidate.Items)
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.OrderId && candidate.UserId == request.UserId,
                cancellationToken);

        return order is null
            ? OperationResultFactory.NotFound<OrderDetailResponse>(message: "Order was not found.")
            : OperationResultFactory.Success(OrderMapping.ToDetail(order));
    }
}