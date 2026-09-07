using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Users.Profile;

public sealed record GetMyUserProfileQuery(string UserId)
    : IRequest<OperationResult<UserProfileResponse>>;