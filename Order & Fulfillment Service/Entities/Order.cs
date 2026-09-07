namespace Order___Fulfillment_Service.Entities;

public sealed class Order
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime PlacedAtUtc { get; set; }
    public OrderStatus Status { get; set; }
    public string DeliveryRecipientName { get; set; } = string.Empty;
    public string DeliveryPhone { get; set; } = string.Empty;
    public string DeliveryAddressLine { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryArea { get; set; } = string.Empty;
    public bool IsGift { get; set; }
    public string? GiftRecipientName { get; set; }
    public string? GiftRecipientPhone { get; set; }
    public string? GiftMessage { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public string? PaymentLast4 { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public List<OrderLineItem> Items { get; set; } = [];
}