namespace Identity_service.Entities;

public sealed class PasswordResetAuditEvent
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;
    public Guid ResetRequestId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string EventType { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public PasswordResetRequest? ResetRequest { get; set; }
}
