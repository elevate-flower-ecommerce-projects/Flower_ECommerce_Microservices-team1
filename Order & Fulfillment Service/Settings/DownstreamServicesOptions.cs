namespace Order___Fulfillment_Service.Settings;

/// <summary>Base URLs of the services checkout calls directly, not through the gateway.</summary>
public sealed class DownstreamServicesOptions
{
    public const string SectionName = "Services";
    public string CartBaseUrl { get; set; } = string.Empty;
    public string AddressBaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}
