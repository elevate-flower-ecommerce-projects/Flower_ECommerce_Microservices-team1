using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Users.Profile;

public sealed record UpdateMyUserProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Gender,
    string? ProfilePictureUrl)
    : IRequest<OperationResult<object>>;
