using Order___Fulfillment_Service.Contracts.Internal;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Features.Internal;

public static class InternalOrderMapping
{
    /// <summary>An order can still be charged while it is a card order that is neither paid nor cancelled.</summary>
    public static bool IsPayable(this Order order)
        => order.PaymentMethod is PaymentMethodType.Card
            && order.PaymentStatus is not PaymentStatus.Paid
            && order.Status is not OrderStatus.Cancelled;

    public static InternalOrderResponse ToInternalResponse(this Order order)
        => new(
            order.Id,
            order.OrderNumber,
            order.CustomerUserId,
            order.Status.ToString(),
            order.PaymentMethod.ToString(),
            order.PaymentStatus.ToString(),
            order.Total,
            order.PaymentStatus is PaymentStatus.Paid,
            order.IsPayable());
}
