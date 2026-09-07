using Order___Fulfillment_Service.Contracts.Orders;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Features.Orders;

public static class OrderMapping
{
    public static OrderListItemResponse ToListItem(Order order)
    {
        var firstItem = order.Items.FirstOrDefault();

        return new OrderListItemResponse(
            order.Id,
            order.OrderNumber,
            order.PlacedAtUtc,
            order.Items.Sum(item => item.Quantity),
            firstItem?.ThumbnailUrl,
            ToStatusResponse(order.Status),
            order.Total);
    }

    public static OrderDetailResponse ToDetail(Order order)
    {
        var canTrack = order.Status is OrderStatus.Placed or OrderStatus.Preparing or OrderStatus.OutForDelivery;

        return new OrderDetailResponse(
            order.Id,
            order.OrderNumber,
            order.PlacedAtUtc,
            ToStatusResponse(order.Status),
            order.Items
                .Select(item => new OrderLineItemResponse(
                    item.ProductId,
                    item.ProductName,
                    item.ThumbnailUrl,
                    item.Quantity,
                    item.UnitPrice,
                    item.LineTotal))
                .ToList(),
            new DeliveryAddressResponse(
                order.DeliveryRecipientName,
                order.DeliveryPhone,
                order.DeliveryAddressLine,
                order.DeliveryCity,
                order.DeliveryArea),
            order.IsGift && !string.IsNullOrWhiteSpace(order.GiftRecipientName) && !string.IsNullOrWhiteSpace(order.GiftRecipientPhone)
                ? new GiftRecipientResponse(order.GiftRecipientName, order.GiftRecipientPhone, order.GiftMessage)
                : null,
            ToPaymentMethodResponse(order),
            new PriceBreakdownResponse(order.Subtotal, order.DeliveryFee, order.Discount, order.Total),
            canTrack,
            canTrack ? $"/orders/{order.Id}/tracking" : null);
    }

    public static OrderStatusResponse ToStatusResponse(OrderStatus status) => status switch
    {
        OrderStatus.Placed => new OrderStatusResponse("placed", "Placed", "#2563EB"),
        OrderStatus.Preparing => new OrderStatusResponse("preparing", "Preparing", "#F59E0B"),
        OrderStatus.OutForDelivery => new OrderStatusResponse("out_for_delivery", "Out for Delivery", "#7C3AED"),
        OrderStatus.Delivered => new OrderStatusResponse("delivered", "Delivered", "#16A34A"),
        OrderStatus.Cancelled => new OrderStatusResponse("cancelled", "Cancelled", "#DC2626"),
        _ => new OrderStatusResponse("placed", "Placed", "#2563EB")
    };

    private static PaymentMethodResponse ToPaymentMethodResponse(Order order) => order.PaymentMethod switch
    {
        PaymentMethodType.Card => new PaymentMethodResponse("card", "Card", order.PaymentLast4),
        PaymentMethodType.Wallet => new PaymentMethodResponse("wallet", "Wallet", order.PaymentLast4),
        PaymentMethodType.CashOnDelivery => new PaymentMethodResponse("cash_on_delivery", "Cash on Delivery", null),
        _ => new PaymentMethodResponse("cash_on_delivery", "Cash on Delivery", null)
    };
}