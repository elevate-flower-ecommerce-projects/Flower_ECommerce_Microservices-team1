namespace Identity_service.Features.Users.UpdateProfile;

public sealed class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;

    /// <summary>Optional. Omitting it keeps the current avatar.</summary>
    public IFormFile? ProfilePicture { get; set; }
}
