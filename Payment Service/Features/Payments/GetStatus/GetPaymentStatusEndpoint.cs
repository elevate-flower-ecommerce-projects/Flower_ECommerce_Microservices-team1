using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Payment_Service.Contracts.Payments;

namespace Payment_Service.Features.Payments.GetStatus;

public sealed class GetPaymentStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(PaymentRoutes.Status, async (
            Guid orderId,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var customerUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerUserId))
            {
                return OperationResultFactory
                    .UnAuthorized(PaymentMessages.MissingIdentity, PaymentMessages.MissingIdentity)
                    .ToHttpResult();
            }

            var result = await sender.Send(new GetPaymentStatusQuery(orderId, customerUserId), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("GetPaymentStatus")
        .WithTags(PaymentRoutes.Tag)
        .Produces<OperationResult<PaymentStatusResponse>>(StatusCodes.Status200OK)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult<PaymentErrorResponse>>(StatusCodes.Status404NotFound)
        .Produces<OperationResult<PaymentErrorResponse>>(StatusCodes.Status503ServiceUnavailable);
    }
}
