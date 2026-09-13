namespace Identity_service.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public Guid FamilyId { get; set; } = Guid.CreateVersion7();
    public string? DeviceInfo { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;

    public ApplicationUser? User { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
}
