namespace Address___Store_Coverage_Service.Entities;

public sealed class StoreCoverageArea
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public decimal? MinLat { get; set; }
    public decimal? MaxLat { get; set; }
    public decimal? MinLng { get; set; }
    public decimal? MaxLng { get; set; }
    public bool IsActive { get; set; } = true;
}
