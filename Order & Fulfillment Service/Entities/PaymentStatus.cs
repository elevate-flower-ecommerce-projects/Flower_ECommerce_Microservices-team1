namespace Order___Fulfillment_Service.Entities;

// A failed card attempt belongs to the payment attempt, not the order: the order stays Pending
// so the customer can try again.
public enum PaymentStatus
{
    Pending = 1,
    Paid = 2
}
