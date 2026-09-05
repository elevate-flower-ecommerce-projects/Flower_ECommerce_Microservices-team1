namespace Order___Fulfillment_Service.Features.Orders;

public static class OrderMessages
{
    public const string MissingIdentity = "Missing user identity.";
    public const string OrderNotFound = "Order was not found.";
    public const string OrderNotAssigned = "You are not assigned to this order.";
    public const string InvalidStatusTransition = "The requested order status transition is not valid.";
    public const string InactiveDelivery = "Driver location updates are only accepted for active deliveries.";
    public const string ValidationFailed = "Driver delivery request validation failed.";
}
