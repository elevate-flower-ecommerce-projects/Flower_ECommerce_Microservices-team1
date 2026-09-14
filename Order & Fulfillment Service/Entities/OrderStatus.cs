namespace Order___Fulfillment_Service.Entities;

public enum OrderStatus
{
    Placed,
    Preparing,
    PickedUp,
    OutForDelivery,
    Delivered,
    Cancelled,

    // Appended, not inserted: clients read these as numbers, so existing values must not shift.
    PendingPayment
}
