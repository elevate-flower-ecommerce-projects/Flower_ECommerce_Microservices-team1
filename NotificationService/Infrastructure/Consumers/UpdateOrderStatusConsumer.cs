using MassTransit;
using MediatR;
using NotificationService.Features.SendUpdateOrderStatusNotification;
using NotificationService.Infrastructure.Contract;

namespace NotificationService.Infrastructure.Consumers;

public class UpdateOrderStatusConsumer(IMediator mediator) : IConsumer<UpdateOrderStatusEvent>
{
    private readonly IMediator _mediator = mediator;

    public async Task Consume(ConsumeContext<UpdateOrderStatusEvent> context)
    {
        var result = await _mediator.Send(new SendUpdateOrderStatusNotificationQuery(
            OrderId: context.Message.OrderId,
            OrderStatus: context.Message.OrderStatus
        ));

    }
}
