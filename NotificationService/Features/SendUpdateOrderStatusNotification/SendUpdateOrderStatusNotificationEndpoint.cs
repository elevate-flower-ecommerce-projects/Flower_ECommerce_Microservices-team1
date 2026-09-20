using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Shared.Interfaces;

namespace NotificationService.Features.SendUpdateOrderStatusNotification;


// It Just For Testing Purpose, You Can Remove It Later
public class SendUpdateOrderStatusNotificationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/notification/send-update-order-status-notification",
            async (SendUpdateOrderStatusNotificationQuery query, [FromServices]IMediator mediator) =>
        {
            var result = await mediator.Send(query, CancellationToken.None);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}
