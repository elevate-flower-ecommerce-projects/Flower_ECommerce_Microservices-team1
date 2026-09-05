using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity_service.Features.Users.UpdateProfile;

public sealed class GetMyProfileHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetMyProfileQuery, OperationResult<UserProfileResponse>>
{
    public async Task<OperationResult<UserProfileResponse>> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        // The token can outlive the account it was issued for.
        if (user is null)
        {
            return OperationResultFactory.NotFound<UserProfileResponse>(
                message: UpdateProfileMessages.UserNotFound,
                messageLocalized: UpdateProfileMessages.UserNotFound);
        }

        var roles = await userManager.GetRolesAsync(user);

        return OperationResultFactory.Success(
            user.ToProfileResponse(roles),
            UpdateProfileMessages.ProfileLoaded,
            UpdateProfileMessages.ProfileLoaded);
    }
}
