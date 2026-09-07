using FirebaseAdmin.Messaging;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;

namespace NotificationService.Features.SendUpdateOrderStatusNotification;

public record SendUpdateOrderStatusNotificationQuery
(string OrderId,
    string OrderStatus
) : IQuery<bool>;

public class SendUpdateOrderStatusNotificationQueryHandler
    : IQueryHandler<SendUpdateOrderStatusNotificationQuery, bool>
{
    public async Task<RequestResult<bool>> Handle(SendUpdateOrderStatusNotificationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var message = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    { "OrderId", request.OrderId },
                    { "OrderStatus", request.OrderStatus }
                },
                Topic = "order_updates"
            };
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return RequestResult<bool>.succeeded(true,ResultCode.NotificationSentSuccesfully);
        }
        catch (FirebaseMessagingException ex)
        {
            return RequestResult<bool>.Failure(ResultCode.NotificationFailedToSent);
        }
    }
}
