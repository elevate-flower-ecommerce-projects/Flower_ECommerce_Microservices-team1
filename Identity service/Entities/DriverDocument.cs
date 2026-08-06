namespace Identity_service.Entities;

/// <summary>
/// Private metadata for documents uploaded with a driver application.
/// </summary>
public class DriverDocument
{
    #region Identity

    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ApplicationId { get; set; }

    #endregion

    #region File metadata

    public string FileUrl { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation properties

    public DriverApplication? Application { get; set; }

    #endregion
}
