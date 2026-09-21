namespace Payment_Service.Entities;

/// <summary>
/// One provider event we have already acted on. The provider retries a webhook until it gets a 2xx,
/// so the same event can arrive many times; the unique index on <see cref="EventId"/> is what makes
/// the second delivery a no-op instead of a second payment.
/// </summary>
public sealed class ProcessedPaymentEvent
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Provider { get; set; } = PaymentProviders.Stripe;
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public Guid? PaymentAttemptId { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}
