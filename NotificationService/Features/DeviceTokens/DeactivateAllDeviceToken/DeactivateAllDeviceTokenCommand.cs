using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;

namespace NotificationService.Features.DeviceTokens.DeactivateAllDeviceToken;

public record DeactivateAllDeviceTokenCommand
(string DeviceId, string UserId) : ICommand<bool>;

public class DeactivateAllDeviceTokenCommandHandler(Repository<DeviceToken> repository)
    : ICommandHandler<DeactivateAllDeviceTokenCommand, bool>
{
    public async Task<RequestResult<bool>> Handle(DeactivateAllDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        // Deactivate every OTHER user's active row on this device — guards
        // against account switching without logout. Must NOT match
        // request.UserId itself; that row is reactivated separately by the
        // orchestrator's update/store step right after this runs.
        var updateResult = await repository.BulkUpdateAsync(
            predicate: dt => dt.DeviceId == request.DeviceId
                              && dt.UserId != request.UserId
                              && dt.IsActive,
            updateProp: dt => dt.IsActive,
            newValue: false,
            cancellationToken: cancellationToken
        );

        // Zero rows affected is the NORMAL case — fresh install, or this
        // device has only ever belonged to request.UserId — never a failure.
        return RequestResult<bool>.succeeded(updateResult > 0, ResultCode.TokenDeactivatedSuccessfully);
    }
}