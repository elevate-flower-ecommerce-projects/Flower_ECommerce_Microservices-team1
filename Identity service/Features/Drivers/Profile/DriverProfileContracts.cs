using Identity_service.Entities;

namespace Identity_service.Features.Drivers.Profile;

public sealed record DriverProfileResponse(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber,
    Gender? Gender,
    string? ProfilePictureUrl,
    VehicleType VehicleType,
    string VehiclePlateNumber,
    string Country);

public sealed class UpdateDriverProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public VehicleType VehicleType { get; set; }
    public string VehiclePlateNumber { get; set; } = string.Empty;
    public string Country { get; set; } = "Egypt";
}