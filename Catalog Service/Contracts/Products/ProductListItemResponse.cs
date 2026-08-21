namespace Catalog_Service.Contracts.Products;

public sealed record ProductListItemResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    decimal? DiscountedPrice,
    decimal? DiscountPercent,
    bool InStock);
