using Flower.Common.StandardizedResponse;
using MediatR;

namespace Identity_service.Features.Users.Register;

public sealed record RegisterCustomerCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Gender,
    string Password,
    string ConfirmPassword) : IRequest<OperationResult<object>>;
