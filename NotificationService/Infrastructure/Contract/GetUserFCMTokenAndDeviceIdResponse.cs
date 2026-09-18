namespace NotificationService.Infrastructure.Contract;

public record GetUserFCMTokenAndDeviceIdResponse
(string FCMToken, string DeviceId);