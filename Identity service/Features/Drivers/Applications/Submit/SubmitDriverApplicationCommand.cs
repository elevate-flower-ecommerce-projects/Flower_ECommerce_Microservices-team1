using Flower.Common.StandardizedResponse;
using Identity_service.Entities;
using MediatR;

namespace Identity_service.Features.Drivers.Applications.Submit;

public sealed record SubmitDriverApplicationCommand(
    string Country,
    string FirstLegalName,
    string SecondLegalName,
    string FullName,
    string Phone,
    string Email,
    string NationalId,
    VehicleType VehicleType,
    string VehiclePlateNumber,
    string Gender,
    string Password,
    string ConfirmPassword,
    IReadOnlyList<IFormFile> Documents) : IRequest<OperationResult<object>>;