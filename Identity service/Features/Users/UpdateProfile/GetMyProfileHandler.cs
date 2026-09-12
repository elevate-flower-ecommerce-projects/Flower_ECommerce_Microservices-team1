using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity_service.Features.Users.UpdateProfile;

public sealed class GetMyProfileHandler(
    UserManager<ApplicationUser> userManager,
    IUnitOfWork<ApplicationDbContext> unitOfWork)
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
        var driverProfile = await GetDriverProfileIfNeededAsync(user.Id, roles, cancellationToken);
        if (roles.Contains(ApplicationRoleNames.Driver) && driverProfile is null)
        {
            return OperationResultFactory.NotFound<UserProfileResponse>(
                message: "Driver profile was not found.",
                messageLocalized: "Driver profile was not found.");
        }

        return OperationResultFactory.Success(
            user.ToProfileResponse(roles, driverProfile: driverProfile),
            UpdateProfileMessages.ProfileLoaded,
            UpdateProfileMessages.ProfileLoaded);
    }

    private async Task<DriverProfile?> GetDriverProfileIfNeededAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken)
    {
        if (!roles.Contains(ApplicationRoleNames.Driver))
            return null;

        return await unitOfWork.Repository<DriverProfile, Guid>()
            .Query(false)
            .SingleOrDefaultAsync(driverProfile => driverProfile.UserId == userId, cancellationToken);
    }
}