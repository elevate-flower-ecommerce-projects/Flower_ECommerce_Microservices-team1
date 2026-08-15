namespace Catalog_Service.Features.Categories.Admin.UpdateCategory;

public sealed record UpdateCategoryRequest(
    string Name,
    string? ImageUrl,
    int? SortOrder);
