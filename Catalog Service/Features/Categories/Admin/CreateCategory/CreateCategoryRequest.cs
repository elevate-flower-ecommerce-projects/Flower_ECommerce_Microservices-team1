namespace Catalog_Service.Features.Categories.Admin.CreateCategory;

public sealed record CreateCategoryRequest(
    string Name,
    string? ImageUrl,
    int? SortOrder,
    bool? IsActive);
