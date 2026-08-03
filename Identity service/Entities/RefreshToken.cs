namespace Identity_service.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOn { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresOn;
    public bool IsActive => RevokedOn is null && !IsExpired;
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }
}
