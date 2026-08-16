namespace Catalog_Service.Entities;

public sealed class Product
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? DiscountPercent { get; set; }
    public DateTime? DiscountStartsAtUtc { get; set; }
    public DateTime? DiscountEndsAtUtc { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? OccasionId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SoldCount { get; set; }

    public ICollection<ProductOccasion> ProductOccasions { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductIncludedItem> IncludedItems { get; set; } = [];
    public ICollection<ProductStoreInventory> StoreInventories { get; set; } = [];
}
