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
            .SingleOrDefaultAsync(driverProfile => driverProfile.UserId == request.UserId, cancellationToken);

        if (profile is null)
            return DriverVehicleLicenseDocuments.DriverProfileNotFound<DriverProfileResponse>();

        var license = await DriverVehicleLicenseDocuments.FindCurrentAsync(dbContext, request.UserId, cancellationToken);

        return OperationResultFactory.Success(DriverVehicleLicenseDocuments.ToProfileResponse(profile, license));
    }
}
