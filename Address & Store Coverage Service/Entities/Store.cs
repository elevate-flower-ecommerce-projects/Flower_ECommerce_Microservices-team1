namespace Address___Store_Coverage_Service.Entities;

public sealed class Store
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public ICollection<StoreCoverageArea> CoverageAreas { get; set; } = [];
}
