using Flower.Common.StandardizedResponse;
using Microsoft.AspNetCore.Mvc;

namespace Identity_service.Features.Drivers.Profile;

/// <summary>
/// The driver app's Vehicle info screen. Account details (name, email, phone, gender, photo) use
/// /users/me/profile, which is shared with customers.
/// </summary>
public sealed class DriverProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/drivers/me/profile")
            .RequireAuthorization(new AuthorizeAttribute { Roles = ApplicationRoleNames.Driver })
            .WithTags("Drivers");

        group.MapGet("", GetAsync)
            .WithName("GetMyDriverProfile")
            .WithSummary("Read the signed-in driver's vehicle info")
            .Produces<OperationResult<DriverProfileResponse>>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status403Forbidden)
            .Produces<OperationResult>(StatusCodes.Status404NotFound);

        group.MapPut("", UpdateAsync)
            .WithName("UpdateMyDriverProfile")
            .WithSummary("Update the signed-in driver's vehicle info")
            .Accepts<UpdateDriverProfileRequest>("multipart/form-data")
            .Produces<OperationResult<DriverProfileResponse>>()
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status403Forbidden)
            .Produces<OperationResult>(StatusCodes.Status404NotFound)
            .DisableAntiforgery();

        group.MapGet("/vehicle-license", DownloadLicenseAsync)
            .WithName("DownloadMyVehicleLicense")
            .WithSummary("Download the signed-in driver's current vehicle license")
            .Produces(StatusCodes.Status200OK)
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status403Forbidden)
            .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return MissingIdentity();

        var result = await sender.Send(new GetMyDriverProfileQuery(userId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateAsync(
        [FromForm] UpdateDriverProfileRequest request,
        ClaimsPrincipal user,
        HttpRequest httpRequest,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return MissingIdentity();

        // Swagger and minimal APIs do not always bind the file onto the DTO, so fall back to the
        // raw multipart payload the same way the profile and application endpoints do.
        var vehicleLicense = request.VehicleLicense;
        if (vehicleLicense is null && httpRequest.HasFormContentType)
        {
            var form = await httpRequest.ReadFormAsync(cancellationToken);
            vehicleLicense = form.Files.GetFile(nameof(UpdateDriverProfileRequest.VehicleLicense))
                ?? form.Files.FirstOrDefault();
        }

        var result = await sender.Send(
            new UpdateMyDriverProfileCommand(userId, request.VehicleType, request.VehicleNumber, vehicleLicense),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> DownloadLicenseAsync(
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        IDriverDocumentStorage documentStorage,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return MissingIdentity();

        // Looked up by the caller's own id, so a driver can never reach another driver's file.
        var license = await DriverVehicleLicenseDocuments.FindCurrentAsync(dbContext, userId, cancellationToken);
        if (license is null)
            return OperationResultFactory.NotFound(message: "Vehicle license was not found.").ToHttpResult();

        var stream = await documentStorage.OpenReadAsync(license.FileUrl, cancellationToken);
        return stream is null
            ? OperationResultFactory.NotFound(message: "Vehicle license file was not found.").ToHttpResult()
            : Results.File(stream, license.ContentType, license.OriginalFileName);
    }

    private static IResult MissingIdentity()
        => OperationResultFactory.UnAuthorized("Missing user identity.", "Missing user identity.").ToHttpResult();
}
