using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Order___Fulfillment_Service.Contracts.Orders;

namespace Order___Fulfillment_Service.Features.Orders.Details;

public sealed class GetCustomerOrderDetailsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResultFactory.UnAuthorized(
                    "Missing user identity.",
                    "Missing user identity.")
                    .ToHttpResult();
            }

            var result = await sender.Send(new GetCustomerOrderDetailsQuery(userId, id), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("GetCustomerOrderDetails")
        .WithTags("Orders")
        .Produces<OperationResult<OrderDetailResponse>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }
}