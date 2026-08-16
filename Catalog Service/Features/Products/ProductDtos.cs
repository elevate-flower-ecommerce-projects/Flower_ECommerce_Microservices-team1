namespace Catalog_Service.Features.Products;

public sealed record ProductSummaryResponse(
    Guid Id,
    string Name,
    string ImageUrl,
    decimal Price,
    bool InStock);

public sealed record PagedResponse<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items);
