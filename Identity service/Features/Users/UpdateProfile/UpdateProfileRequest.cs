namespace Identity_service.Features.Users.UpdateProfile;

/// <summary>
/// Mirrors the Edit profile screen, which is the same in the customer and driver apps. Vehicle
/// details have their own screen and endpoint (/drivers/me/profile).
/// </summary>
public sealed class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Nullable so a missing value is reported as required instead of defaulting.</summary>
    public Gender? Gender { get; set; }

    /// <summary>Optional. Omitting it keeps the current avatar.</summary>
    public IFormFile? ProfilePicture { get; set; }
}
