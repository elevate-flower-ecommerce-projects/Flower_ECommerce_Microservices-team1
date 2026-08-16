namespace Catalog_Service.Contracts.Products;

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<string> ImageUrls,
    IReadOnlyList<ProductIncludedItemResponse> IncludedItems,
    decimal Price,
    decimal? DiscountedPrice,
    decimal? DiscountPercent,
    bool RequiresStoreSelection,
    bool InStock,
    int AvailableQuantity);

public sealed record ProductIncludedItemResponse(
    string Name,
    int Quantity);
