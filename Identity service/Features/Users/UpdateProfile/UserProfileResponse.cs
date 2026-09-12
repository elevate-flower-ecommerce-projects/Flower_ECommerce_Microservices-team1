using System.Text.Json.Serialization;

namespace Identity_service.Features.Users.UpdateProfile;

/// <summary>
/// The vehicle fields only exist for drivers, so they are left out of the JSON entirely for
/// customers and admins instead of being sent as null.
/// </summary>
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
    bool EmailChanged,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? VehicleType,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? VehiclePlateNumber,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Country);
