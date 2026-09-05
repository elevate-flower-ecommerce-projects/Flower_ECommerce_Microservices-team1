namespace Order___Fulfillment_Service.Entities;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerUserId { get; set; } = string.Empty;
    public string? AssignedDriverUserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAtUtc { get; set; }

    public Guid? StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreAddress { get; set; } = string.Empty;
    public decimal StoreLatitude { get; set; }
    public decimal StoreLongitude { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string DeliveryAddressLine { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryArea { get; set; } = string.Empty;
    public decimal DeliveryLatitude { get; set; }
    public decimal DeliveryLongitude { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<DriverLocation> DriverLocations { get; set; } = new List<DriverLocation>();
}
