using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Drivers.Profile;

/// <summary>
/// Driver documents are stored on the driver's application. The profile screen shows the latest
/// available document as the vehicle license so image uploads are not hidden just because their
/// stored document type is IdentityImage.
/// </summary>
internal static class DriverVehicleLicenseDocuments
{
    /// <summary>Document type written when the driver replaces the license from the profile.</summary>
    public const string VehicleLicense = "VehicleLicense";

    public const string DownloadUrl = "/drivers/me/profile/vehicle-license";

    /// <summary>The most recent driver-owned document wins, regardless of its stored document type.</summary>
    public static Task<DriverDocument?> FindCurrentAsync(
        ApplicationDbContext dbContext,
        string userId,
        CancellationToken cancellationToken)
        => dbContext.Set<DriverDocument>()
            .AsNoTracking()
            .Where(document => document.Application != null
                && document.Application.UserId == userId)
            .OrderByDescending(document => document.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public static DriverVehicleLicenseResponse? ToResponse(DriverDocument? document)
        => document is null
            ? null
            : new DriverVehicleLicenseResponse(
                document.Id,
                document.OriginalFileName,
                document.ContentType,
                document.UploadedAt,
                DownloadUrl);

    public static DriverProfileResponse ToProfileResponse(DriverProfile profile, DriverDocument? license)
        => new(profile.VehicleType, profile.PlateNumber, ToResponse(license));

    public static OperationResult<T> DriverProfileNotFound<T>()
        => OperationResultFactory.NotFound<T>(
            message: "Driver profile was not found.",
            messageLocalized: "Driver profile was not found.");
}