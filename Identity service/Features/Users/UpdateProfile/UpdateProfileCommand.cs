using Flower.Common.StandardizedResponse;
using MediatR;

namespace Identity_service.Features.Users.UpdateProfile;

public sealed record UpdateProfileCommand(
    string UserId,
    string FullName,
    string Email,
    string PhoneNumber,
    string Gender,
    IFormFile? ProfilePicture) : IRequest<OperationResult<object>>;
