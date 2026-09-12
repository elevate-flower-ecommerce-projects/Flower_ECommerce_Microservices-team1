using System.Text.Json.Serialization;

namespace Identity_service.Features.Users.UpdateProfile;

/// <summary>
/// Account data shared by every role. Vehicle details are returned by /drivers/me/profile.
/// </summary>
public sealed record UserProfileResponse(
    string UserId,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? PhoneNumber,
    [property: JsonConverter(typeof(JsonStringEnumConverter<Gender>))] Gender? Gender,
    string? ProfilePictureUrl,
    IReadOnlyList<string> Roles,
    bool EmailChanged);
