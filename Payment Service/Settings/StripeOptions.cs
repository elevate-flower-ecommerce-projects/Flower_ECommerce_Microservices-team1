namespace Payment_Service.Settings;

/// <summary>
/// Stripe credentials and the URLs the customer lands on after the hosted checkout page.
/// The secrets belong in user-secrets or environment variables and must never be committed.
/// </summary>
public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    /// <summary>Test-mode secret key (starts with sk_test_).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Signing secret of the webhook endpoint (starts with whsec_).</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>Three-letter currency code the amounts are charged in.</summary>
    public string Currency { get; set; } = "egp";

    /// <summary>Where the hosted page sends the customer back. Display only; it never marks an order paid.</summary>
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>How long a hosted checkout session stays open, in minutes (Stripe allows 30 to 1440).</summary>
    public int SessionLifetimeMinutes { get; set; } = 30;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SecretKey);
}
