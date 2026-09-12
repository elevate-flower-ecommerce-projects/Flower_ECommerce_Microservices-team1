using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Drivers.Profile;

public sealed record UpdateMyDriverProfileCommand(
    string UserId,
    VehicleType? VehicleType,
    string VehicleNumber,
    IFormFile? VehicleLicense)
    : IRequest<OperationResult<object>>;
