using System.Text.Json.Serialization;

namespace Identity_service.Features.Users.UpdateProfile;

/// <summary>
/// Account data shared by every role. Driver callers also receive vehicle details for the profile screen.
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
    bool EmailChanged,
    [property: JsonConverter(typeof(JsonStringEnumConverter<VehicleType>))] VehicleType? VehicleType,
    string? VehiclePlateNumber,
    string? Country);