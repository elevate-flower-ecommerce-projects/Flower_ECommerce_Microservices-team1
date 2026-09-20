using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;
using NotificationService.Shared.Services;

namespace NotificationService.Features.SendUpdateOrderStatusNotification;

public record SendUpdateOrderStatusNotificationQuery
(string OrderId,
    string OrderStatus ,string UserId
) : IQuery<bool>;


public class SendUpdateOrderStatusNotificationQueryHandler(Repository<DeviceToken> repository)
    : IQueryHandler<SendUpdateOrderStatusNotificationQuery, bool>
{
    public async Task<RequestResult<bool>> Handle(SendUpdateOrderStatusNotificationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tokens = await repository.Get(t => t.UserId == request.UserId && t.IsActive)
                                   .Select(t => t.FcmToken).ToListAsync();

            if (tokens is null || tokens.Count == 0)
            {
                return RequestResult<bool>.Failure(ResultCode.NotificationFailedToSent);
            }

            var multicastMessage = new MulticastMessage
            {
                Tokens = tokens,
                Data = new Dictionary<string, string>
                {
                    { "orderId", request.OrderId },
                    { "orderStatus", request.OrderStatus },
                    { "updateType", "STATUS_CHANGED" }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(
                multicastMessage, cancellationToken);

            // Optional but recommended: prune invalid tokens (AC#4)
            if (response.FailureCount > 0)
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var result = response.Responses[i];
                    if (!result.IsSuccess &&
                        result.Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    {
                        // TODO: notify Auth service (or your own token store) to remove tokens[i]
                    }
                }
            }

            return RequestResult<bool>.succeeded(true, ResultCode.NotificationSentSuccesfully);
        }
        catch (FirebaseMessagingException)
        {
            return RequestResult<bool>.Failure(ResultCode.NotificationFailedToSent);
        }
    }
}


//public class SendUpdateOrderStatusNotificationQueryHandler(AuthServiceClient authServiceClient)
//    : IQueryHandler<SendUpdateOrderStatusNotificationQuery, bool>
//{
//    private readonly AuthServiceClient _authServiceClient = authServiceClient;

//    public async Task<RequestResult<bool>> Handle(SendUpdateOrderStatusNotificationQuery request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var tokens = await _authServiceClient.GetDeviceTokensAsync(request.UserId);

//            if (tokens is null || tokens.Count == 0)
//            {
//                return RequestResult<bool>.Failure(ResultCode.NotificationFailedToSent);
//            }

//            var multicastMessage = new MulticastMessage
//            {
//                Tokens = tokens,
//                Data = new Dictionary<string, string>
//                {
//                    { "orderId", request.OrderId },
//                    { "orderStatus", request.OrderStatus },
//                    { "updateType", "STATUS_CHANGED" }
//                }
//            };

//            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(
//                multicastMessage, cancellationToken);

//            // Optional but recommended: prune invalid tokens (AC#4)
//            if (response.FailureCount > 0)
//            {
//                for (int i = 0; i < response.Responses.Count; i++)
//                {
//                    var result = response.Responses[i];
//                    if (!result.IsSuccess &&
//                        result.Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered)
//                    {
//                        // TODO: notify Auth service (or your own token store) to remove tokens[i]
//                    }
//                }
//            }

//            return RequestResult<bool>.succeeded(true, ResultCode.NotificationSentSuccesfully);
//        }
//        catch (FirebaseMessagingException)
//        {
//            return RequestResult<bool>.Failure(ResultCode.NotificationFailedToSent);
//        }
//    }
//}
