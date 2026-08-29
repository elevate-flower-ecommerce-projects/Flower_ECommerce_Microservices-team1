namespace Cart_Service.Settings;

public sealed class CatalogOptions
{
    public const string SectionName = "Catalog";
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}
