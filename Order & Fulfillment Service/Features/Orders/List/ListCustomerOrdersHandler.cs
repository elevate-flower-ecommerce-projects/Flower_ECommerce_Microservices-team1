using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Orders;
using Order___Fulfillment_Service.Entities;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;
using System.Linq.Expressions;

namespace Order___Fulfillment_Service.Features.Orders.List;

public sealed class ListCustomerOrdersHandler(IUnitOfWork<OrderDbContext> unitOfWork)
    : IRequestHandler<ListCustomerOrdersQuery, OperationResult<PagedResponse<OrderListItemResponse>>>
{
    public async Task<OperationResult<PagedResponse<OrderListItemResponse>>> Handle(
        ListCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;

        Expression<Func<Order, bool>> predicate = order => order.UserId == request.UserId;

        var paged = await unitOfWork.Repository<Order, Guid>()
            .GetPageSelectAsync(
                page,
                pageSize,
                predicate,
                order => new OrderListItemResponse(
                    order.Id,
                    order.OrderNumber,
                    order.PlacedAtUtc,
                    order.Items.Sum(item => item.Quantity),
                    order.Items
                        .OrderBy(item => item.Id)
                        .Select(item => item.ThumbnailUrl)
                        .FirstOrDefault(),
                    OrderMapping.ToStatusResponse(order.Status),
                    order.Total),
                query => query.OrderByDescending(order => order.PlacedAtUtc).ThenByDescending(order => order.Id));

        return OperationResultFactory.Success(
            new PagedResponse<OrderListItemResponse>(page, pageSize, paged.TotalCount, paged.Items));
    }
}