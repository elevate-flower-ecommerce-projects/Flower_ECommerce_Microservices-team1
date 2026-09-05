using System.Security.Claims;
using Flower.Common.StandardizedResponse;
using Microsoft.AspNetCore.Mvc;

namespace Identity_service.Features.Users.UpdateProfile;

/// <summary>
/// Reads and updates the signed-in user's own profile. Open to every authenticated role, since
/// customers, drivers and admins all have a name, email, phone and avatar.
/// </summary>
public sealed class UpdateProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users/me/profile")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", GetAsync)
            .WithName("GetMyProfile")
            .WithSummary("Read the signed-in user's profile")
            .Produces<OperationResult<UserProfileResponse>>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status404NotFound);

        group.MapPut("/", UpdateAsync)
            .WithName("UpdateMyProfile")
            .WithSummary("Update the signed-in user's profile")
            .Accepts<UpdateProfileRequest>("multipart/form-data")
            .Produces<OperationResult<UserProfileResponse>>()
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status404NotFound)
            .DisableAntiforgery();
    }

    private static async Task<IResult> GetAsync(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
            return UnauthorizedResult();

        var result = await sender.Send(new GetMyProfileQuery(userId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateAsync(
        [FromForm] UpdateProfileRequest request,
        ClaimsPrincipal user,
        HttpRequest httpRequest,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
            return UnauthorizedResult();

        // Swagger and minimal APIs do not always bind the file onto the DTO, so fall back to the
        // raw multipart payload the same way the driver application endpoint does.
        var profilePicture = request.ProfilePicture;
        if (profilePicture is null && httpRequest.HasFormContentType)
        {
            var form = await httpRequest.ReadFormAsync(cancellationToken);
            profilePicture = form.Files.GetFile(nameof(UpdateProfileRequest.ProfilePicture))
                ?? form.Files.FirstOrDefault();
        }

        var result = await sender.Send(new UpdateProfileCommand(
            userId,
            request.FullName,
            request.Email,
            request.PhoneNumber,
            request.Gender,
            profilePicture), cancellationToken);

        return result.ToHttpResult();
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out string userId)
    {
        // Login writes the id as "sub"; the JWT handler surfaces it as NameIdentifier.
        userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(userId);
    }

    private static IResult UnauthorizedResult()
        => OperationResultFactory.UnAuthorized(
            UpdateProfileMessages.MissingIdentity,
            UpdateProfileMessages.MissingIdentity)
            .ToHttpResult();
}
