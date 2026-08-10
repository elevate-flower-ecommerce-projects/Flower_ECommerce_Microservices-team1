namespace Catalog_Service.Entities;

public sealed class HomeSection
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;
    public string ContentRefJson { get; set; } = "{}";
}
