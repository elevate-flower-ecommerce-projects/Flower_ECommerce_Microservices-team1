using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Users.Profile;

public sealed class UserProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users/me/profile")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
            .WithTags("Users");

        group.MapGet("", GetAsync)
            .WithName("GetMyUserProfile")
            .Produces<OperationResult<UserProfileResponse>>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status403Forbidden)
            .Produces<OperationResult>(StatusCodes.Status404NotFound);

        group.MapPut("", UpdateAsync)
            .WithName("UpdateMyUserProfile")
            .Accepts<UpdateUserProfileRequest>("application/json")
            .Produces<OperationResult<UserProfileResponse>>()
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

        var result = await sender.Send(new GetMyUserProfileQuery(userId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateAsync(UpdateUserProfileRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return MissingIdentity();

        var result = await sender.Send(new UpdateMyUserProfileCommand(userId, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Gender, request.ProfilePictureUrl), cancellationToken);
        return result.ToHttpResult();
    }

    private static IResult MissingIdentity()
        => OperationResultFactory.UnAuthorized("Missing user identity.", "Missing user identity.").ToHttpResult();
}