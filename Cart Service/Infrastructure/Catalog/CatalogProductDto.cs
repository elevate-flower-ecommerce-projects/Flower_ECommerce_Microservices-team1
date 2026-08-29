namespace Cart_Service.Infrastructure.Catalog;

/// <summary>
/// Mirrors <c>Catalog_Service.Contracts.Products.ProductDetailResponse</c>. Only the fields the
/// cart needs are declared; the rest of the payload is ignored on deserialization.
/// </summary>
public sealed record CatalogProductDto(
    Guid Id,
    string Name,
    IReadOnlyList<string> ImageUrls,
    decimal Price,
    decimal? DiscountedPrice,
    bool RequiresStoreSelection,
    bool InStock,
    int AvailableQuantity)
{
    /// <summary>The price a customer actually pays right now.</summary>
    public decimal EffectivePrice => DiscountedPrice ?? Price;
}
