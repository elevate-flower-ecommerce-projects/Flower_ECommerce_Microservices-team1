namespace Identity_service.Features.Users.UpdateProfile;

public sealed record UserProfileResponse(
    string UserId,
    string FullName,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Gender,
    string? ProfilePictureUrl,
    IReadOnlyList<string> Roles,
    bool EmailChanged);
