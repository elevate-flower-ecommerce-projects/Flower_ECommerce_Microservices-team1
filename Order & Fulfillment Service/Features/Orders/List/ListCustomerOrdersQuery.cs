using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Orders;

namespace Order___Fulfillment_Service.Features.Orders.List;

public sealed record ListCustomerOrdersQuery(
    string UserId,
    int Page,
    int PageSize) : IRequest<OperationResult<PagedResponse<OrderListItemResponse>>>;