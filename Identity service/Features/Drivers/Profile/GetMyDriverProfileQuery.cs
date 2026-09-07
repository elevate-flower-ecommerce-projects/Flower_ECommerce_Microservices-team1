using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Drivers.Profile;

public sealed record GetMyDriverProfileQuery(string UserId)
    : IRequest<OperationResult<DriverProfileResponse>>;