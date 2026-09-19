using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;

namespace NotificationService.Features.DeviceTokens.StoreNewDeviceToken;

public record UpdateDeviceTokenCommand
(Guid Id, string Token, DateTime RefreshTokenExpiresAt)
    : ICommand<bool>;

public class UpdateDeviceTokenCommandHandler(Repository<DeviceToken> repository)
    : ICommandHandler<UpdateDeviceTokenCommand, bool>
{
    public async Task<RequestResult<bool>> Handle(UpdateDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        repository.SaveInclude(new DeviceToken
        {
            Id = request.Id,
            FcmToken = request.Token,
            RefreshTokenExpiresAt = request.RefreshTokenExpiresAt,
            IsActive = true,
            LastSeenAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        nameof(DeviceToken.FcmToken),
        nameof(DeviceToken.UpdatedAt),
        nameof(DeviceToken.RefreshTokenExpiresAt),
        nameof(DeviceToken.IsActive),
        nameof(DeviceToken.LastSeenAt));

        var affectedRows = await repository.SaveChangeAsync(cancellationToken);
        if (affectedRows == 0)
            return RequestResult<bool>.Failure(ResultCode.DeviceTokenNotFound);

        return RequestResult<bool>.succeeded(true, ResultCode.DeviceTokenUpdatedSuccessfully);
    }
}