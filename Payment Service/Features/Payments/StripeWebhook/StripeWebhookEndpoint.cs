using Carter;
using MediatR;

namespace Payment_Service.Features.Payments.StripeWebhook;

/// <summary>
/// Called by Stripe, not by the mobile app. It is open to the internet on purpose: the signature
/// header is what authenticates it, and a body without a valid one is refused.
/// </summary>
public sealed class StripeWebhookEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(PaymentRoutes.StripeWebhook, async (
            HttpRequest httpRequest,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            // Read as raw text: the signature is computed over exactly these bytes, so the body must
            // not be deserialized and re-serialized before it is verified.
            using var reader = new StreamReader(httpRequest.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);
            var signature = httpRequest.Headers["Stripe-Signature"].FirstOrDefault();

            var result = await sender.Send(new StripeWebhookCommand(payload, signature), cancellationToken);

            return result switch
            {
                WebhookResult.Acknowledged => Results.Ok(new { received = true }),
                WebhookResult.Rejected => Results.BadRequest(new { received = false }),
                _ => Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
            };
        })
        .AllowAnonymous()
        .WithName("StripeWebhook")
        .ExcludeFromDescription()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status503ServiceUnavailable);
    }
}
