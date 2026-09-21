using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Payment_Service.Contracts.Payments;

namespace Payment_Service.Features.Payments.CreateSession;

public sealed class CreatePaymentSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(PaymentRoutes.CreateSession, async (
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

            var result = await sender.Send(new CreatePaymentSessionCommand(orderId, customerUserId), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("CreatePaymentSession")
        .WithTags(PaymentRoutes.Tag)
        .Produces<OperationResult<PaymentSessionResponse>>(StatusCodes.Status201Created)
        .Produces<OperationResult<PaymentSessionResponse>>(StatusCodes.Status200OK)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult<PaymentErrorResponse>>(StatusCodes.Status404NotFound)
        .Produces<OperationResult<PaymentErrorResponse>>(StatusCodes.Status409Conflict)
        .Produces<OperationResult<PaymentErrorResponse>>(StatusCodes.Status503ServiceUnavailable);
    }
}
