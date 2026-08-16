namespace Catalog_Service.Contracts.Products;

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    Guid? CategoryId,
    Guid? OccasionId,
    Guid? StoreId,
    bool IsAvailable,
    int SoldCount);
