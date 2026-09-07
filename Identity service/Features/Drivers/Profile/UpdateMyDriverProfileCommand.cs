using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Drivers.Profile;

public sealed record UpdateMyDriverProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Gender,
    string? ProfilePictureUrl,
    VehicleType VehicleType,
    string VehiclePlateNumber,
    string Country)
    : IRequest<OperationResult<object>>;