using Identity_service.Entities;

namespace Identity_service.Features.Users.Profile;

public sealed record UserProfileResponse(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber,
    Gender? Gender,
    string? ProfilePictureUrl);

public sealed class UpdateUserProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}