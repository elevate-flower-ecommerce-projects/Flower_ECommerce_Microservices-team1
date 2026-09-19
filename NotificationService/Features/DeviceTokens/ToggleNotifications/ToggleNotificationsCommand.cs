using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;

namespace NotificationService.Features.DeviceTokens.ToggleNotifications;

public record ToggleNotificationsCommand
(string DeviceId, string UserId, bool Enabled)
    : ICommand<ToggleNotificationsResponse>;

public record ToggleNotificationsResponse(bool Found, bool NotificationsEnabled);

public class ToggleNotificationsCommandHandler(Repository<DeviceToken> repository)
    : ICommandHandler<ToggleNotificationsCommand, ToggleNotificationsResponse>
{
    public async Task<RequestResult<ToggleNotificationsResponse>> Handle(
        ToggleNotificationsCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository
            .Get(dt => dt.DeviceId == request.DeviceId && dt.UserId == request.UserId)
            .Select(dt => new { dt.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
            return RequestResult<ToggleNotificationsResponse>.Failure(ResultCode.DeviceTokenNotFound);

        // Touches ONLY NotificationsEnabled — IsActive is a session/technical
        // concern this slice must never modify.
        repository.SaveInclude(new DeviceToken
        {
            Id = existing.Id,
            NotificationsEnabled = request.Enabled,
            UpdatedAt = DateTime.UtcNow
        },
        nameof(DeviceToken.NotificationsEnabled),
        nameof(DeviceToken.UpdatedAt));

        var affectedRows = await repository.SaveChangeAsync(cancellationToken);
        if (affectedRows == 0)
            return RequestResult<ToggleNotificationsResponse>.Failure(ResultCode.DeviceTokenNotFound);

        return RequestResult<ToggleNotificationsResponse>.succeeded(
            new ToggleNotificationsResponse(true, request.Enabled),
            ResultCode.NotificationsToggledSuccessfully);
    }
}