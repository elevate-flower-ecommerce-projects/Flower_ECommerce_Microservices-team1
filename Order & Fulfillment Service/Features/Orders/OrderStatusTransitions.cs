using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Features.Orders;

public static class OrderStatusTransitions
{
    public static bool IsValidDriverTransition(OrderStatus current, OrderStatus requested)
        => (current, requested) switch
        {
            (OrderStatus.PickedUp, OrderStatus.OutForDelivery) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Delivered) => true,
            _ => false
        };

    public static bool IsActiveDelivery(OrderStatus status)
        => status is OrderStatus.PickedUp or OrderStatus.OutForDelivery;
}
