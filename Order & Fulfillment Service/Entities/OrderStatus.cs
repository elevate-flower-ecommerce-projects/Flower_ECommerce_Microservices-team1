namespace Order___Fulfillment_Service.Entities;

public enum OrderStatus
{
    Placed = 1,
    Preparing = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Cancelled = 5
}