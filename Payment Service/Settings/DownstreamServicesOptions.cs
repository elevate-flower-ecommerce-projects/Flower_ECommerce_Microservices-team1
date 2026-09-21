namespace Payment_Service.Settings;

/// <summary>Base URLs of the services this one calls directly, not through the gateway.</summary>
public sealed class DownstreamServicesOptions
{
    public const string SectionName = "Services";
    public string OrderBaseUrl { get; set; } = string.Empty;
    public string CartBaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}
