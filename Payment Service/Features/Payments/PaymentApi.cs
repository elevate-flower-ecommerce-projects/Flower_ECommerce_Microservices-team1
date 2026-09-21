namespace Payment_Service.Features.Payments;

public static class PaymentRoutes
{
    // The gateway strips "/api/v1/payments", so these are reachable as /api/v1/payments/orders/...
    public const string CreateSession = "/orders/{orderId:guid}/session";
    public const string Status = "/orders/{orderId:guid}/status";
    public const string StripeWebhook = "/stripe/webhook";
    public const string Tag = "Payments";
}

public static class PaymentMessages
{
    public const string MissingIdentity = "Missing user identity.";
    public const string SessionCreated = "Payment session created successfully.";
    public const string SessionReused = "An open payment session already exists for this order.";
    public const string StatusLoaded = "Payment status loaded successfully.";
    public const string OrderNotFound = "Order was not found.";
    public const string OrderNotYours = "Order was not found.";
    public const string OrderAlreadyPaid = "This order is already paid.";
    public const string OrderNotPayable = "This order cannot be paid online.";
    public const string InvalidAmount = "The order total cannot be charged.";
    public const string PaymentUnavailable = "The payment provider is temporarily unavailable. Please try again.";
    public const string PaymentNotConfigured = "Online payment is not configured on this environment.";
    public const string OrdersUnavailable = "Order information is temporarily unavailable. Please try again.";
}

/// <summary>Stable codes the mobile app switches on, so it never has to match on message text.</summary>
public static class PaymentErrorCodes
{
    public const string OrderNotFound = nameof(OrderNotFound);
    public const string OrderAlreadyPaid = nameof(OrderAlreadyPaid);
    public const string OrderNotPayable = nameof(OrderNotPayable);
    public const string PaymentUnavailable = nameof(PaymentUnavailable);
    public const string DependencyUnavailable = nameof(DependencyUnavailable);
}
