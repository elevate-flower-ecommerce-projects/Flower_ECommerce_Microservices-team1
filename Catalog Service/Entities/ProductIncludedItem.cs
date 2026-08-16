namespace Catalog_Service.Entities;

public sealed class ProductIncludedItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int SortOrder { get; set; }
    public Product Product { get; set; } = null!;
}
