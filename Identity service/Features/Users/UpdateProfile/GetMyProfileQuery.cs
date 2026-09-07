using Flower.Common.StandardizedResponse;
using MediatR;

namespace Identity_service.Features.Users.UpdateProfile;

public sealed record GetMyProfileQuery(string UserId) : IRequest<OperationResult<UserProfileResponse>>;
