namespace Catalog_Service.Entities;

public sealed class Banner
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string DeepLink { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
