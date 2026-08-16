namespace Catalog_Service.Contracts.Products;

public sealed record ProductListItemResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    Guid? CategoryId,
    Guid? OccasionId,
    bool IsAvailable);
