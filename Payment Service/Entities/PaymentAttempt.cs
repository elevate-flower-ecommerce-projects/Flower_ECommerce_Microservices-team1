namespace Payment_Service.Entities;

/// <summary>
/// One attempt to pay one order through the provider. The amount is copied from the order when the
/// attempt is created, never taken from the client, and the provider's event is checked against it.
/// </summary>
public sealed class PaymentAttempt
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerUserId { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;

    public string Provider { get; set; } = PaymentProviders.Stripe;
    public string? ProviderSessionId { get; set; }
    public string? ProviderPaymentIntentId { get; set; }
    public string? CheckoutUrl { get; set; }

    public PaymentAttemptStatus Status { get; set; } = PaymentAttemptStatus.Pending;
    public string? FailureReason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>Set once the order service has been told about a successful payment.</summary>
    public DateTime? OrderNotifiedAtUtc { get; set; }

    public bool IsFinal => Status is not PaymentAttemptStatus.Pending;
}

public static class PaymentProviders
{
    public const string Stripe = "Stripe";
}
