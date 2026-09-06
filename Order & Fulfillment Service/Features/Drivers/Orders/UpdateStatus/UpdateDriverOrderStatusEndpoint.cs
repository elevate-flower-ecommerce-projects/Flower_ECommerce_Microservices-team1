using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Order___Fulfillment_Service.Contracts.Drivers;
using Order___Fulfillment_Service.Contracts.Orders;
using Order___Fulfillment_Service.Features.Orders;

namespace Order___Fulfillment_Service.Features.Drivers.Orders.UpdateStatus;

public sealed class UpdateDriverOrderStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(OrderRoutes.OrderStatus, async (Guid id, UpdateOrderStatusRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
        {
            var driverUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(driverUserId))
                return OperationResultFactory.UnAuthorized(OrderMessages.MissingIdentity, OrderMessages.MissingIdentity).ToHttpResult();

            return (await sender.Send(new UpdateDriverOrderStatusCommand(id, driverUserId, request.Status), cancellationToken)).ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Driver" })
        .WithName("UpdateDriverOrderStatus")
        .WithTags(OrderRoutes.Tag)
        .Produces<OperationResult<DriverOrderDetailsResponse>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult<DriverOrderDetailsResponse>>(StatusCodes.Status409Conflict)
        .Produces<OperationResult<DriverOrderDetailsResponse>>(StatusCodes.Status422UnprocessableEntity);
    }
}
