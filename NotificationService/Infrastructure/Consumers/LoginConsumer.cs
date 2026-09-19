using MassTransit;
using MediatR;
using NotificationService.Features.DeviceTokens.StoreNewDeviceToken;
using Shared.Events.Identity;

namespace NotificationService.Infrastructure.Consumers;

public class LoginConsumer(IMediator mediator, ILogger<LoginConsumer> logger) : IConsumer<LoginEvent>
{
    public async Task Consume(ConsumeContext<LoginEvent> context)
    {
        var upsertResult = await mediator.Send(
            new StoreNewDeviceTokenOrchestrator(
                context.Message.DeviceId,
                context.Message.UserId,
                context.Message.FCMToken,
                context.Message.RefreshTokenExpiresAt),
            context.CancellationToken);

        if (!upsertResult.Success)
        {
            logger.LogError(
                "Failed to store device token for user {UserId} / device {DeviceId}. Error: {ErrorMessage}",
                context.Message.UserId, context.Message.DeviceId, upsertResult.Message);

            throw new InvalidOperationException(
                $"Device token upsert failed: {upsertResult.Code}");
        }
    }
}