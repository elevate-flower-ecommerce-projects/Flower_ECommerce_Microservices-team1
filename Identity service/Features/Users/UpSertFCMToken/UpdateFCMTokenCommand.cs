using Identity_service.Errors;
using Identity_service.Infrastructure.Persistence.Repositories;

namespace Identity_service.Features.Users.UpSertFCMToken;

public record UpdateFCMTokenCommand
(Guid Id, string FCMToken) : IRequest<Result<bool>>;

public class UpdateFCMTokenCommandHandler(Repository<UserFCMToken> repository
    ,ILogger<UpdateFCMTokenCommandHandler> logger)
    : IRequestHandler<UpdateFCMTokenCommand, Result<bool>>
{
    private readonly Repository<UserFCMToken> _repository = repository;
    private readonly ILogger<UpdateFCMTokenCommandHandler> _logger = logger;

    public async Task<Result<bool>> Handle(UpdateFCMTokenCommand request, CancellationToken cancellationToken)
    {
        var userFCMToken = new UserFCMToken
        {
            Id = request.Id,
            FCMToken = request.FCMToken,
        };
        _repository.SaveInclude(userFCMToken, nameof(UserFCMToken.FCMToken));
        _repository.Add(userFCMToken);
        try
        {
            var affectedRows = await _repository.SaveChangeAsync(cancellationToken);
            if (affectedRows == 0)
                return Result.Failure<bool>(UserErrors.CanNotInsertFCMToken);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed To Update FCM Token For {TokenId} due to {ex}", request.Id, ex.Message);
            return Result.Failure<bool>(UserErrors.CanNotInsertFCMToken);
        }
    }
}
