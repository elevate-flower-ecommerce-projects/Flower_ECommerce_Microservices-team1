using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Drivers.Profile;

public sealed class DriverProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/drivers/me/profile")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Driver" })
            .WithTags("Drivers");

        group.MapGet("", GetAsync)
            .WithName("GetMyDriverProfile")
            .Produces<OperationResult<DriverProfileResponse>>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status403Forbidden)
            .Produces<OperationResult>(StatusCodes.Status404NotFound);

        group.MapPut("", UpdateAsync)
            .WithName("UpdateMyDriverProfile")
            .Accepts<UpdateDriverProfileRequest>("application/json")
            .Produces<OperationResult<DriverProfileResponse>>()
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
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

    private static async Task<IResult> UpdateAsync(UpdateDriverProfileRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return MissingIdentity();

        var result = await sender.Send(new UpdateMyDriverProfileCommand(userId, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Gender, request.ProfilePictureUrl, request.VehicleType, request.VehiclePlateNumber, request.Country), cancellationToken);
        return result.ToHttpResult();
    }

    private static IResult MissingIdentity()
        => OperationResultFactory.UnAuthorized("Missing user identity.", "Missing user identity.").ToHttpResult();
}