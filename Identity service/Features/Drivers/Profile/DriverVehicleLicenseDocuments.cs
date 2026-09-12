using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Drivers.Profile;

/// <summary>
/// The vehicle license is stored as a <see cref="DriverDocument"/> on the driver's application,
/// next to the documents uploaded when applying, so no new table is needed and admins keep the
/// full history.
/// </summary>
internal static class DriverVehicleLicenseDocuments
{
    /// <summary>Document type written when the driver replaces the license from the profile.</summary>
    public const string VehicleLicense = "VehicleLicense";

    /// <summary>Document type the application flow gives to PDF uploads, which are licenses.</summary>
    public const string ApplicationLicensePdf = "LicensePdf";

    public const string DownloadUrl = "/drivers/me/profile/vehicle-license";

    /// <summary>The most recent license wins, whether it came from the application or the profile.</summary>
    public static Task<DriverDocument?> FindCurrentAsync(
        ApplicationDbContext dbContext,
        string userId,
        CancellationToken cancellationToken)
        => dbContext.Set<DriverDocument>()
            .AsNoTracking()
            .Where(document => document.Application != null
                && document.Application.UserId == userId
                && (document.DocType == VehicleLicense || document.DocType == ApplicationLicensePdf))
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
