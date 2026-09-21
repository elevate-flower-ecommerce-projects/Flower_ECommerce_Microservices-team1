namespace Order___Fulfillment_Service.Settings;

/// <summary>
/// The shared secret other Flower services present when they call an internal endpoint. A payment
/// provider's webhook carries no customer token, so the payment service has to reach this service
/// as itself; this secret is what tells the two apart from anyone else on the network.
/// It belongs in user-secrets or environment variables and must never be committed.
/// </summary>
public sealed class InternalApiOptions
{
    public const string SectionName = "Internal";
    public const string DefaultHeaderName = "X-Internal-Secret";

    public string Secret { get; set; } = string.Empty;
    public string HeaderName { get; set; } = DefaultHeaderName;
}
