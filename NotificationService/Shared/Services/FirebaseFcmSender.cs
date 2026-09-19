using FirebaseAdmin.Messaging;
using NotificationService.Shared.Response;

namespace NotificationService.Shared.Services;

public sealed class FirebaseFcmSender(ILogger<FirebaseFcmSender> logger)
{
    public async Task<FcmSendResult> SendAsync(
        string fcmToken,
        string title,
        string body,
        IDictionary<string, string>? data,
        CancellationToken cancellationToken)
    {
        var message = new Message
        {
            Token = fcmToken,
            Notification = new Notification { Title = title, Body = body },
            Data = data?.AsReadOnly(),
            Android = new AndroidConfig { Priority = Priority.High },
            Apns = new ApnsConfig { Aps = new Aps { ContentAvailable = true } }
        };

        try
        {
            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
            logger.LogInformation("FCM push sent ({MessageId}) to token ending {TokenSuffix}", messageId, Suffix(fcmToken));
            return FcmSendResult.Sent;
        }
        catch (FirebaseMessagingException ex) when (
            ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
            ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
        {
            // Token is dead or malformed — this is the ONE case the design doc
            // says should deactivate the row. Everything else must not.
            logger.LogInformation(ex, "FCM reports invalid token (ending {TokenSuffix}): {ErrorCode}", Suffix(fcmToken), ex.MessagingErrorCode);
            return FcmSendResult.InvalidToken;
        }
        catch (FirebaseMessagingException ex)
        {
            // Quota, transient server error, etc. — not evidence the token
            // itself is bad. Leave the row untouched; retry later.
            logger.LogWarning(ex, "Transient FCM failure (ending {TokenSuffix}): {ErrorCode}", Suffix(fcmToken), ex.MessagingErrorCode);
            return FcmSendResult.TransientFailure;
        }
    }

    // Never log a full FCM token.
    private static string Suffix(string token) => token.Length <= 6 ? token : token[^6..];
}