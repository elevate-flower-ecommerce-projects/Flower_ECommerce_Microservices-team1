namespace Order___Fulfillment_Service.Features.Checkout;

public static class CheckoutMessages
{
    public const string SummaryReady = "Checkout summary calculated.";
    public const string OrderPlaced = "Order placed successfully.";
    public const string OrderAlreadyPlaced = "This order was already placed.";

    public const string ValidationFailed = "Checkout request validation failed.";
    public const string IdempotencyKeyRequired = "The Idempotency-Key header is required and must not exceed 100 characters.";
    public const string AddressNotFound = "Address was not found.";
    public const string AddressRequired = "Add a delivery address to continue.";
    public const string AddressNotServiceable = "We do not deliver to this address yet.";
    public const string CartEmpty = "Your cart is empty.";
    public const string ItemsUnavailable = "Some items are not available at the store that serves this address.";
    public const string PriceChanged = "Prices changed since you reviewed your order. Please review the new total.";
    public const string DependencyUnavailable = "Checkout is temporarily unavailable. Please try again.";
}

public static class CheckoutErrorCodes
{
    public const string AddressRequired = "AddressRequired";
    public const string AddressNotServiceable = "AddressNotServiceable";
    public const string CartEmpty = "CartEmpty";
    public const string ItemsUnavailable = "ItemsUnavailable";
    public const string PriceChanged = "PriceChanged";
    public const string DependencyUnavailable = "DependencyUnavailable";
}
