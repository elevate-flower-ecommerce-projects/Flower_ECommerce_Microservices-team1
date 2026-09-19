using MediatR;
using NotificationService.Features.DeviceTokens.DeactivateAllDeviceToken;
using NotificationService.Features.DeviceTokens.FindFirstTokenByDeviceId;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;

namespace NotificationService.Features.DeviceTokens.StoreNewDeviceToken;

public record StoreNewDeviceTokenOrchestrator
(
    string DeviceId,
    string UserId,
    string Token,
    DateTime RefreshTokenExpiresAt
) : ICommand<bool>;

public class StoreNewDeviceTokenOrchestratorHandler(IMediator mediator)
    : ICommandHandler<StoreNewDeviceTokenOrchestrator, bool>
{
    public async Task<RequestResult<bool>> Handle(StoreNewDeviceTokenOrchestrator request, CancellationToken cancellationToken)
    {
        var deactivateResult = await mediator.Send( new DeactivateAllDeviceTokenCommand(request.DeviceId,
            request.UserId));
        var existingTokenResult = await mediator.Send(new FindFirstTokenByDeviceIdQuery(request.DeviceId, 
            request.UserId),cancellationToken);
        if(existingTokenResult.Success && existingTokenResult.Result != null)
        {
            var updateResult = await mediator.Send(new UpdateDeviceTokenCommand(existingTokenResult.Result.Id, request.Token, request.RefreshTokenExpiresAt), cancellationToken);
            if (updateResult.Success)
                return RequestResult<bool>.succeeded(true,ResultCode.DeviceTokenUpdatedSuccessfully);
  
            return RequestResult<bool>.Failure(updateResult.Code);
        }

        var storeResult = await mediator.Send(new StoreNewDeviceTokenCommand(request.DeviceId, request.UserId, request.Token, request.RefreshTokenExpiresAt), cancellationToken);
        if (storeResult.Success)
            return RequestResult<bool>.succeeded(true,ResultCode.DeviceTokenStoredSuccessfully);

        return RequestResult<bool>.Failure(storeResult.Code);
    }
}