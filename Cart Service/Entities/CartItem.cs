namespace Cart_Service.Entities;

public sealed class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    /// <summary>
    /// Snapshot of the catalog product taken when the item was added, so the cart can still be
    /// rendered when the Catalog service is unreachable and so price changes can be detected.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPriceSnapshot { get; set; }

    public DateTime AddedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Cart Cart { get; set; } = null!;
}
