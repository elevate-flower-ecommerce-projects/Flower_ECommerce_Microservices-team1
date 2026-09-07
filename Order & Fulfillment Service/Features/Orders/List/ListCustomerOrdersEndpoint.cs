using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Order___Fulfillment_Service.Contracts.Orders;

namespace Order___Fulfillment_Service.Features.Orders.List;

public sealed class ListCustomerOrdersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResultFactory.UnAuthorized(
                    "Missing user identity.",
                    "Missing user identity.")
                    .ToHttpResult();
            }

            var result = await sender.Send(new ListCustomerOrdersQuery(userId, page, pageSize), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("ListCustomerOrders")
        .WithTags("Orders")
        .Produces<OperationResult<PagedResponse<OrderListItemResponse>>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden);
    }
}