namespace Identity_service.Features.Users.GetFCMToken;

public record GetFCMTokenQuery
(string UserId , string DeviceId , string FCMToken) :
    IRequest<Result<string>>;