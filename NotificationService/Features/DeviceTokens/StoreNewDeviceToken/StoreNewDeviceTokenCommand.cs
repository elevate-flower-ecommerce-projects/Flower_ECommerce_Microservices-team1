using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;

namespace NotificationService.Features.DeviceTokens.StoreNewDeviceToken;

public record StoreNewDeviceTokenCommand
(
    string DeviceId,
    string UserId,
    string Token,
    DateTime RefreshTokenExpiresAt
) : ICommand<bool>;

public class StoreNewDeviceTokenCommandHandler(Repository<DeviceToken> repository)
    : ICommandHandler<StoreNewDeviceTokenCommand, bool>
{
    public async Task<RequestResult<bool>> Handle(StoreNewDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var newDeviceToken = new DeviceToken
        {
            DeviceId = request.DeviceId,
            UserId = request.UserId,
            FcmToken = request.Token,
            IsActive = true,
            IsDeleted = false,
            LastSeenAt = DateTime.UtcNow,
            NotificationsEnabled = true, // default only on a brand-new row
            RefreshTokenExpiresAt = request.RefreshTokenExpiresAt
        };

        repository.Add(newDeviceToken);
        var affectedRows = await repository.SaveChangeAsync(cancellationToken);

        if (affectedRows <= 0)
            return RequestResult<bool>.Failure(ResultCode.DeviceTokenFailedToStore);

        return RequestResult<bool>.succeeded(true, ResultCode.DeviceTokenStoredSuccessfully);
    }
}