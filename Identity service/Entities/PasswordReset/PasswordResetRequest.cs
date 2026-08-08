namespace Identity_service.Entities;

public sealed class PasswordResetRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;
    public string OtpHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime LastSentAtUtc { get; set; }
    public int AttemptsRemaining { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public DateTime? InvalidatedAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
    public string? ResetTokenHash { get; set; }
    public DateTime? ResetTokenExpiresAtUtc { get; set; }
    public ApplicationUser? User { get; set; }
    public ICollection<PasswordResetAuditEvent> AuditEvents { get; set; } = [];
}
