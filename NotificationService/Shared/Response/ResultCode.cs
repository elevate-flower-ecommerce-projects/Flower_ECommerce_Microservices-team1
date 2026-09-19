namespace NotificationService.Shared.Response;

public enum ResultCode
{
    NotificationSentSuccesfully = 100,
    NotificationFailedToSent = 101,


    //Device Token
    TokenDeactivatedSuccessfully = 200,
    NoActiveTokensFound = 201,
    DeviceTokenNotFound = 202,
    DeviceTokenFoundSuccessfully = 203,
    DeviceTokenUpdatedSuccessfully = 204,
    DeviceTokenStoredSuccessfully = 205,
    DeviceTokenFailedToStore = 206,
    NotificationsToggledSuccessfully = 207,
}
