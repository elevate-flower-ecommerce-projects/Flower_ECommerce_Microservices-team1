namespace Identity_service.Settings;

public class DriverDocumentStorageOptions
{
    public const string SectionName = "DriverDocuments";

    public string RootPath { get; set; } = "App_Data/driver-documents";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "application/pdf"
    ];
}
