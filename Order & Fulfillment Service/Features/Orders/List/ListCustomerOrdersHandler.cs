using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Orders;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;

namespace Order___Fulfillment_Service.Features.Orders.List;

public sealed class ListCustomerOrdersHandler(OrderDbContext dbContext)
    : IRequestHandler<ListCustomerOrdersQuery, OperationResult<PagedResponse<OrderListItemResponse>>>
{
    public async Task<OperationResult<PagedResponse<OrderListItemResponse>>> Handle(
        ListCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;

        var query = dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(order => order.PlacedAtUtc)
            .ThenByDescending(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var response = new PagedResponse<OrderListItemResponse>(
            page,
            pageSize,
            totalCount,
            orders.Select(OrderMapping.ToListItem).ToList());

        return OperationResultFactory.Success(response);
    }
}