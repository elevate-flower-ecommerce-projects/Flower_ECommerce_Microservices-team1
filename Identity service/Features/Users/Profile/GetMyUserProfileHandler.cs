using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Users.Profile;

public sealed class GetMyUserProfileHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetMyUserProfileQuery, OperationResult<UserProfileResponse>>
{
    public async Task<OperationResult<UserProfileResponse>> Handle(
        GetMyUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return OperationResultFactory.NotFound<UserProfileResponse>(
                message: "User profile was not found.",
                messageLocalized: "User profile was not found.");
        }

        return OperationResultFactory.Success(ToResponse(user));
    }

    internal static UserProfileResponse ToResponse(ApplicationUser user)
        => new(
            user.Id,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            user.Gender,
            user.ProfilePictureUrl);
}
