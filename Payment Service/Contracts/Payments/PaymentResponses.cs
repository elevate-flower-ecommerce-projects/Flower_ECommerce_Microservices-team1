namespace Payment_Service.Contracts.Payments;

/// <summary>
/// What the mobile app needs to open the hosted payment page. The amount is the one the order
/// service reported, not anything the client sent.
/// </summary>
public sealed record PaymentSessionResponse(
    Guid PaymentAttemptId,
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string Currency,
    string CheckoutUrl,
    DateTime ExpiresAtUtc,
    bool Reused);

public sealed record PaymentErrorResponse(string Code);

/// <summary>
/// Where the payment for one order stands. The order's own status is the truth; the attempt fields
/// describe the last try, and CheckoutUrl is filled only while a page is still usable.
/// </summary>
public sealed record PaymentStatusResponse(
    Guid OrderId,
    string OrderNumber,
    string OrderStatus,
    string PaymentStatus,
    bool IsPaid,
    decimal Amount,
    string Currency,
    Guid? PaymentAttemptId,
    string? AttemptStatus,
    string? CheckoutUrl,
    DateTime? ExpiresAtUtc,
    bool CanRetry);
