namespace NotificationService.Infrastructure.Contract;

public class UpdateOrderStatusEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
}
