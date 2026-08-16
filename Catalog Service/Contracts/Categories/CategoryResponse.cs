namespace Catalog_Service.Contracts.Categories;

/// <summary>
/// Public shape of a category. Mirrors <c>CategorySummary</c> used by the home
/// category rail so the client renders both from the same contract.
/// </summary>
public sealed record CategoryResponse(Guid Id, string Name, string? ImageUrl, string DeepLink);

/// <summary>
/// Admin shape. Adds the fields an administrator needs but customers never see.
/// </summary>
public sealed record AdminCategoryResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    int SortOrder,
    bool IsActive);
