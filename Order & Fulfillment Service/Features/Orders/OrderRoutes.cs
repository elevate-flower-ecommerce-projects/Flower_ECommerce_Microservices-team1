namespace Order___Fulfillment_Service.Features.Orders;

public static class OrderRoutes
{
    public const string DriverOrders = "/drivers/me/orders";
    public const string DriverOrder = "/drivers/me/orders/{id:guid}";
    public const string DriverLocation = "/drivers/me/location";
    public const string OrderStatus = "/orders/{id:guid}/status";
    public const string Tag = "Driver Orders";

    // The gateway strips "/api/v1/orders", so these are reachable as /api/v1/orders/checkout...
    public const string CheckoutPreview = "/checkout/preview";
    public const string Checkout = "/checkout";
    public const string CheckoutTag = "Checkout";
}
