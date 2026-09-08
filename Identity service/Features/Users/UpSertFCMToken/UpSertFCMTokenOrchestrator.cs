using Identity_service.Features.Users.GetFCMToken;
using Identity_service.Features.Users.UpSertFCMToken;
using Identity_service.Infrastructure.Persistence.Repositories;

namespace Identity_service.Features.Users.StoreFCMToken;

public record UpSertFCMTokenOrchestrator(string UserId, string DeviceId, string FCMToken) :
    IRequest<Result<bool>>;

public sealed class StoreFCMTokenHandler(Repository<UserFCMToken> repository,
    ILogger<StoreFCMTokenHandler> logger ,IMediator mediator)
    : IRequestHandler<UpSertFCMTokenOrchestrator, Result<bool>>
{
    private readonly Repository<UserFCMToken> _repository = repository;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<bool>> Handle(
        UpSertFCMTokenOrchestrator request,
        CancellationToken cancellationToken)
    {
        //var userId = Guid.Parse(request.UserId);
        var existingToken = await _mediator.Send(new GetFCMTokenQuery(request.UserId, request.DeviceId), cancellationToken);
        if(existingToken.IsFailure)
        {
            var addedResult = await _mediator.Send(new CreateUserFCMTokenCommand(request.UserId, request.FCMToken
                , request.DeviceId), cancellationToken);
            return addedResult;
        }
       var updateResult = await _mediator.Send(new UpdateFCMTokenCommand(existingToken.Value.Id, request.FCMToken));
       return updateResult;
    }
}
