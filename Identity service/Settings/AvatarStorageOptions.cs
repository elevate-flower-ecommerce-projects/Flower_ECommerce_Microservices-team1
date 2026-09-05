namespace Identity_service.Settings;

public class AvatarStorageOptions
{
    public const string SectionName = "Avatars";

    /// <summary>Path under wwwroot. Avatars are public, unlike driver documents.</summary>
    public string RelativePath { get; set; } = "uploads/avatars";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png"
    ];
}
