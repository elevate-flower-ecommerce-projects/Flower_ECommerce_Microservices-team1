namespace Catalog_Service.Entities;

public sealed class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? OccasionId { get; set; }
    public Guid? StoreId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public int SoldCount { get; set; }
}
