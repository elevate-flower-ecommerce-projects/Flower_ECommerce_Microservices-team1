namespace NotificationService.Domain;

public class DeviceToken:BaseEntity
{
    public Guid DeviceId { get; set; }
    public string UserId { get; set; }
    public string? FcmToken { get; set; }
    public bool IsActive { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime LastSeenAt { get; set; }
}
