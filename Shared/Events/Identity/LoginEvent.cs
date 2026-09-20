namespace Shared.Events.Identity;

public record LoginEvent
(string UserId, string DeviceId, string FCMToken ,DateTime RefreshTokenExpiresAt);
