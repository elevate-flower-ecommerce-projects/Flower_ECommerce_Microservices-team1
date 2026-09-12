using Flower.Common.StandardizedResponse;
using MediatR;

namespace Identity_service.Features.Users.UpdateProfile;

public sealed record UpdateProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Gender? Gender,
    IFormFile? ProfilePicture) : IRequest<OperationResult<object>>;
