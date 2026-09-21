using MediatR;

namespace Payment_Service.Features.Payments.StripeWebhook;

public enum WebhookResult
{
    /// <summary>Acknowledged. Stripe stops retrying, whether or not the event changed anything.</summary>
    Acknowledged,

    /// <summary>The body did not come from Stripe, or could not be verified.</summary>
    Rejected,

    /// <summary>We could not finish; Stripe should send it again.</summary>
    Retry
}

/// <summary>The raw body matters: the signature is computed over the exact bytes Stripe sent.</summary>
public sealed record StripeWebhookCommand(string Payload, string? SignatureHeader) : IRequest<WebhookResult>;
