using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Order___Fulfillment_Service.Contracts.Drivers;
using Order___Fulfillment_Service.Features.Orders;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.Details;

public sealed class GetDriverOrderDetailsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(OrderRoutes.DriverOrder, async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
        {
            var driverUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(driverUserId))
                return OperationResultFactory.UnAuthorized(OrderMessages.MissingIdentity, OrderMessages.MissingIdentity).ToHttpResult();

            return (await sender.Send(new GetDriverOrderDetailsQuery(id, driverUserId), cancellationToken)).ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Driver" })
        .WithName("GetDriverOrderDetails")
        .WithTags(OrderRoutes.Tag)
        .Produces<OperationResult<DriverOrderDetailsResponse>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }
}
