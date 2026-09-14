using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Order___Fulfillment_Service.Contracts.Checkout;
using Order___Fulfillment_Service.Features.Orders;

namespace Order___Fulfillment_Service.Features.Checkout.Preview;

public sealed class PreviewCheckoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // The body is optional: an empty body means "use my default address".
        app.MapPost(OrderRoutes.CheckoutPreview, async (
            CheckoutPreviewRequest? request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new PreviewCheckoutQuery(request?.AddressId, request?.Gift), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("PreviewCheckout")
        .WithTags(OrderRoutes.CheckoutTag)
        .Produces<OperationResult<CheckoutSummaryResponse>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult<CheckoutErrorResponse>>(StatusCodes.Status409Conflict)
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
        .Produces<OperationResult<CheckoutErrorResponse>>(StatusCodes.Status503ServiceUnavailable);
    }
}
