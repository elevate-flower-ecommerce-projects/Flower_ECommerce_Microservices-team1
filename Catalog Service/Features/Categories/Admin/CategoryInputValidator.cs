namespace Catalog_Service.Features.Categories.Admin;

/// <summary>
/// Field checks shared by category create and update.
/// Returns field-keyed messages so the client can highlight the offending input.
/// </summary>
internal static class CategoryInputValidator
{
    public const int NameMaxLength = 120;
    public const int ImageUrlMaxLength = 512;

    public static Dictionary<string, string[]> Validate(
        string? name,
        string? imageUrl,
        int? sortOrder)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var trimmedName = name?.Trim() ?? string.Empty;

        if (trimmedName.Length == 0)
            errors["Name"] = ["Category name is required."];
        else if (trimmedName.Length > NameMaxLength)
            errors["Name"] = [$"Category name must not exceed {NameMaxLength} characters."];

        if (imageUrl is { Length: > ImageUrlMaxLength })
            errors["ImageUrl"] = [$"Image URL must not exceed {ImageUrlMaxLength} characters."];

        if (sortOrder is < 0)
            errors["SortOrder"] = ["Sort order must be zero or greater."];

        return errors;
    }
}
