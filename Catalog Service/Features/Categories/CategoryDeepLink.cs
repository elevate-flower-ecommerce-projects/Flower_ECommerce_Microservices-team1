namespace Catalog_Service.Features.Categories;

/// <summary>
/// Single source of truth for the category deep link handed to clients.
/// </summary>
internal static class CategoryDeepLink
{
    public static string For(Guid categoryId) => $"/categories/{categoryId}";
}
