using Identity_service.Errors;
using Identity_service.Infrastructure.Persistence.Repositories;

namespace Identity_service.Features.Users.GetFCMToken;

public record GetFCMTokenQuery
(string UserId , string DeviceId) :
    IRequest<Result<GetFCMTokenResponse>>;

public record GetFCMTokenResponse(Guid Id, string Token);

public class GetFCMTokenQueryHandler(Repository<UserFCMToken> repository, ILogger<GetFCMTokenQueryHandler> logger)
    : IRequestHandler<GetFCMTokenQuery, Result<GetFCMTokenResponse>>
{
    private readonly Repository<UserFCMToken> _repository = repository;
    private readonly ILogger<GetFCMTokenQueryHandler> _logger = logger;

    public async Task<Result<GetFCMTokenResponse>> Handle(GetFCMTokenQuery request, CancellationToken cancellationToken)
    {
        var token =  await _repository
             .Get(t => t.UserId == request.UserId && t.DeviceId == request.DeviceId)
             .Select(t =>new GetFCMTokenResponse( t.Id,t.FCMToken))
             .FirstOrDefaultAsync(cancellationToken);
        if (token == null)
            return Result.Failure<GetFCMTokenResponse>(UserErrors.FCMTokenNotFoundForThisDevice);
        return Result.Success(token);
    }
}