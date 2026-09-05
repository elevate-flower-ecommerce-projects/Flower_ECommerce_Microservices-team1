using Microsoft.AspNetCore.Identity;

namespace Identity_service.Entities;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7().ToString();
        SecurityStamp = Guid.CreateVersion7().ToString();
    }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Relative URL of the avatar served from wwwroot, for example /uploads/avatars/{guid}.jpg.
    /// Null until the user uploads one.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public CustomerProfile? CustomerProfile { get; set; }
    public DriverProfile? DriverProfile { get; set; }
    public ICollection<DriverApplication> DriverApplications { get; set; } = [];
    public Gender? Gender { get; set; }
    public ICollection<PasswordResetRequest> PasswordResetRequests { get; set; } = [];
    public ICollection<PasswordResetAuditEvent> PasswordResetAuditEvents { get; set; } = [];
}
