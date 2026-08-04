namespace Identity_service.Entities;

/// <summary>
/// Review workflow record for a prospective driver application.
/// </summary>
public class DriverApplication
{
    #region Identity

    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;

    #endregion

    #region Review state

    public DriverApplicationStatus Status { get; set; } = DriverApplicationStatus.PendingReview;
    public string? RejectionReason { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation properties

    public ApplicationUser? User { get; set; }
    public ICollection<DriverDocument> Documents { get; set; } = [];

    #endregion
}
