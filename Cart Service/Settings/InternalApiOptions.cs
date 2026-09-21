namespace Cart_Service.Settings;

/// <summary>
/// The shared secret other Flower services present when they call an internal endpoint. After a card
/// payment the confirmation arrives from a provider webhook with no customer token, so the payment
/// service clears the cart as itself. Keep the value in user-secrets or environment variables.
/// </summary>
public sealed class InternalApiOptions
{
    public const string SectionName = "Internal";
    public const string DefaultHeaderName = "X-Internal-Secret";

    public string Secret { get; set; } = string.Empty;
    public string HeaderName { get; set; } = DefaultHeaderName;
}
