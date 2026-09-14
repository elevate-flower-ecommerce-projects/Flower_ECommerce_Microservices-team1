using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order___Fulfillment_Service.Contracts.Checkout;
using Order___Fulfillment_Service.Features.Orders;

namespace Order___Fulfillment_Service.Features.Checkout.PlaceOrder;

public sealed class PlaceOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(OrderRoutes.Checkout, async (
            PlaceOrderRequest? request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var customerUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerUserId))
                return OperationResultFactory.UnAuthorized(OrderMessages.MissingIdentity, OrderMessages.MissingIdentity).ToHttpResult();

            var command = new PlaceOrderCommand(
                customerUserId,
                idempotencyKey,
                request ?? new PlaceOrderRequest(null, null, null, null));

            return (await sender.Send(command, cancellationToken)).ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("PlaceOrder")
        .WithTags(OrderRoutes.CheckoutTag)
        .Produces<OperationResult<PlacedOrderResponse>>(StatusCodes.Status201Created)
        .Produces<OperationResult>(StatusCodes.Status400BadRequest)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult<CheckoutErrorResponse>>(StatusCodes.Status409Conflict)
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
        .Produces<OperationResult<CheckoutErrorResponse>>(StatusCodes.Status503ServiceUnavailable);
    }
}
