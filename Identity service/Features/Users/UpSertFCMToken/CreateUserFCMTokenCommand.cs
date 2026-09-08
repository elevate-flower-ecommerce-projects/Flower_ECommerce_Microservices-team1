using Identity_service.Errors;
using Identity_service.Infrastructure.Persistence.Repositories;

namespace Identity_service.Features.Users.UpSertFCMToken;

public record CreateUserFCMTokenCommand
(string UserId, string FCMToken, string DeviceId)
    : IRequest<Result<bool>>;

public class CreateUserFCMTokenCommandHandler(Repository<UserFCMToken> repository
    ,ILogger<CreateUserFCMTokenCommandHandler> logger)
    : IRequestHandler<CreateUserFCMTokenCommand, Result<bool>>
{
    private readonly Repository<UserFCMToken> _repository = repository;
    private readonly ILogger<CreateUserFCMTokenCommandHandler> _logger = logger;

    public async Task<Result<bool>> Handle(CreateUserFCMTokenCommand request, CancellationToken cancellationToken)
    {
        var userFCMToken = new UserFCMToken
        {
            FCMToken = request.FCMToken,
            UserId = request.UserId,
            DeviceId = request.DeviceId
        };
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
            _logger.LogError("Failed To Insert FCM Token For this User {userId} due to {ex}", request.UserId, ex.Message);
            return Result.Failure<bool>(UserErrors.CanNotInsertFCMToken);
        }
    }
}
