using System.Text.Json.Serialization;
using Identity_service.Entities;

namespace Identity_service.Features.Drivers.Profile;

/// <summary>
/// Mirrors the driver app's Vehicle info screen: vehicle type, vehicle number and vehicle license.
/// Name, email, phone, gender and photo are edited through /users/me/profile like every other role.
/// </summary>
public sealed record DriverProfileResponse(
    [property: JsonConverter(typeof(JsonStringEnumConverter<VehicleType>))] VehicleType VehicleType,
    string VehicleNumber,
    DriverVehicleLicenseResponse? VehicleLicense);

/// <summary>
/// The license is a private document, so the app downloads it through an authorized endpoint
/// instead of a public link.
/// </summary>
public sealed record DriverVehicleLicenseResponse(
    Guid DocumentId,
    string FileName,
    string ContentType,
    DateTime UploadedAt,
    string DownloadUrl);

public sealed class UpdateDriverProfileRequest
{
    /// <summary>Nullable so a missing value is reported as required instead of defaulting to Motorcycle.</summary>
    public VehicleType? VehicleType { get; set; }

    public string VehicleNumber { get; set; } = string.Empty;

    /// <summary>Optional jpg, png or pdf. Omitting it keeps the current license.</summary>
    public IFormFile? VehicleLicense { get; set; }
}
