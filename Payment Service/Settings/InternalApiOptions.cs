namespace Payment_Service.Settings;

/// <summary>
/// The shared secret this service presents when it calls another Flower service's internal
/// endpoints. It must match the value those services are configured with, and it belongs in
/// user-secrets or environment variables, never in the repository.
/// </summary>
public sealed class InternalApiOptions
{
    public const string SectionName = "Internal";
    public const string DefaultHeaderName = "X-Internal-Secret";

    public string Secret { get; set; } = string.Empty;
    public string HeaderName { get; set; } = DefaultHeaderName;
}
