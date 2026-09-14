namespace Order___Fulfillment_Service.Entities;

// Starts at 1 so a missing value (0) is rejected instead of silently meaning Cash on Delivery.
public enum PaymentMethodType
{
    CashOnDelivery = 1,
    Card = 2,
    Wallet = 3
}
