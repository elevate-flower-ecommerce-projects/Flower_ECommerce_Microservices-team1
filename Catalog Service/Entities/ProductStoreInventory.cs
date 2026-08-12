namespace Catalog_Service.Entities;

public sealed class ProductStoreInventory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid StoreId { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Product Product { get; set; } = null!;
}
