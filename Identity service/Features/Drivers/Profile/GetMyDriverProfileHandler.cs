using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Drivers.Profile;

public sealed class GetMyDriverProfileHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetMyDriverProfileQuery, OperationResult<DriverProfileResponse>>
{
    public async Task<OperationResult<DriverProfileResponse>> Handle(
        GetMyDriverProfileQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.DriverProfiles
            .AsNoTracking()
            .Include(driverProfile => driverProfile.User)
            .SingleOrDefaultAsync(driverProfile => driverProfile.UserId == request.UserId, cancellationToken);

        if (profile?.User is null)
        {
            return OperationResultFactory.NotFound<DriverProfileResponse>(
                message: "Driver profile was not found.",
                messageLocalized: "Driver profile was not found.");
        }

        return OperationResultFactory.Success(ToResponse(profile.User, profile));
    }

    internal static DriverProfileResponse ToResponse(ApplicationUser user, DriverProfile profile)
        => new(
            user.Id,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            user.Gender,
            user.ProfilePictureUrl,
            profile.VehicleType,
            profile.PlateNumber,
            profile.Country);
}