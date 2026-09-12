using Flower.Common.StandardizedResponse;
using Identity_service.Features.Users;
using Microsoft.Extensions.Options;

namespace Identity_service.Features.Drivers.Profile;

/// <summary>
/// Updates the driver's vehicle: type, number and, optionally, a new license document. Account
/// fields are not touched here; they belong to /users/me/profile.
/// </summary>
public sealed class UpdateMyDriverProfileHandler(
    ApplicationDbContext dbContext,
    IDriverDocumentStorage documentStorage,
    IOptions<DriverDocumentStorageOptions> documentOptions,
    ILogger<UpdateMyDriverProfileHandler> logger)
    : IRequestHandler<UpdateMyDriverProfileCommand, OperationResult<object>>
{
    private const int MaxVehicleNumberLength = 32;

    public async Task<OperationResult<object>> Handle(
        UpdateMyDriverProfileCommand request,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            return Validation(errors);

        var profile = await dbContext.DriverProfiles
            .SingleOrDefaultAsync(driverProfile => driverProfile.UserId == request.UserId, cancellationToken);

        if (profile is null)
            return DriverVehicleLicenseDocuments.DriverProfileNotFound<object>();

        profile.VehicleType = request.VehicleType!.Value;
        profile.PlateNumber = request.VehicleNumber.Trim();

        if (request.VehicleLicense is not null)
        {
            // The license is kept with the application documents so admins see every version.
            var applicationId = await dbContext.Set<DriverApplication>()
                .Where(application => application.UserId == request.UserId)
                .OrderByDescending(application => application.SubmittedAt)
                .Select(application => (Guid?)application.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (applicationId is null)
            {
                logger.LogWarning("Driver {UserId} has a profile but no application to attach the license to.", request.UserId);
                return OperationResultFactory.NotFound<object>(
                    message: "Driver application was not found.",
                    messageLocalized: "Driver application was not found.");
            }

            var stored = await documentStorage.SaveAsync(applicationId.Value, request.VehicleLicense, cancellationToken);
            dbContext.Set<DriverDocument>().Add(new DriverDocument
            {
                ApplicationId = applicationId.Value,
                FileUrl = stored.StorageKey,
                DocType = DriverVehicleLicenseDocuments.VehicleLicense,
                OriginalFileName = stored.OriginalFileName,
                ContentType = stored.ContentType,
                SizeInBytes = stored.SizeInBytes
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var license = await DriverVehicleLicenseDocuments.FindCurrentAsync(dbContext, request.UserId, cancellationToken);

        return OperationResultFactory.Success<object>(
            DriverVehicleLicenseDocuments.ToProfileResponse(profile, license),
            "Vehicle info updated successfully.",
            "Vehicle info updated successfully.");
    }

    private Dictionary<string, string[]> Validate(UpdateMyDriverProfileCommand request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var vehicleNumber = request.VehicleNumber?.Trim() ?? string.Empty;

        UserProfileFieldRules.AddIf(errors, nameof(request.VehicleType), request.VehicleType is null, "Vehicle type is required.");
        UserProfileFieldRules.AddIf(
            errors,
            nameof(request.VehicleType),
            request.VehicleType is { } vehicleType && !Enum.IsDefined(vehicleType),
            "Vehicle type must be Motorcycle or Car.");

        UserProfileFieldRules.AddIf(errors, nameof(request.VehicleNumber), vehicleNumber.Length == 0, "Vehicle number is required.");
        UserProfileFieldRules.AddIf(
            errors,
            nameof(request.VehicleNumber),
            vehicleNumber.Length > MaxVehicleNumberLength,
            "Vehicle number must not exceed 32 characters.");

        ValidateLicense(request.VehicleLicense, errors);

        return errors;
    }

    private void ValidateLicense(IFormFile? file, Dictionary<string, string[]> errors)
    {
        if (file is null)
            return;

        var options = documentOptions.Value;
        var allowedContentTypes = options.AllowedContentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        const string field = nameof(UpdateMyDriverProfileCommand.VehicleLicense);

        UserProfileFieldRules.AddIf(errors, field, file.Length <= 0, "The selected file is empty.");
        UserProfileFieldRules.AddIf(
            errors,
            field,
            file.Length > options.MaxFileSizeBytes,
            $"The file exceeds the {options.MaxFileSizeBytes / (1024 * 1024)}MB size limit.");

        // The declared content type comes from the client, so the file signature is checked too.
        UserProfileFieldRules.AddIf(
            errors,
            field,
            file.Length > 0 && (!allowedContentTypes.Contains(file.ContentType) || !HasAllowedSignature(file)),
            "The vehicle license must be a jpg, png or pdf file.");
    }

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PdfSignature = [0x25, 0x50, 0x44, 0x46];

    private static bool HasAllowedSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[8];

        using var stream = file.OpenReadStream();
        var read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

        return StartsWith(header, read, PngSignature)
            || StartsWith(header, read, JpegSignature)
            || StartsWith(header, read, PdfSignature);
    }

    private static bool StartsWith(ReadOnlySpan<byte> header, int read, byte[] signature)
        => read >= signature.Length && header[..signature.Length].SequenceEqual(signature);

    private static OperationResult<object> Validation(Dictionary<string, string[]> errors)
        => OperationResultFactory.Validation<object>(errors, "Vehicle info validation failed.", "Vehicle info validation failed.");
}
