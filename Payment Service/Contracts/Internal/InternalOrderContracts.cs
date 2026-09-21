namespace Payment_Service.Contracts.Internal;

/// <summary>
/// The order service's answer about one order. Statuses arrive as names, and IsPaid / IsPayable are
/// decided there, so this service never re-implements the order's own rules.
/// </summary>
public sealed record InternalOrderSnapshot(
    Guid OrderId,
    string OrderNumber,
    string CustomerUserId,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    decimal Total,
    bool IsPaid,
    bool IsPayable);

public sealed record MarkOrderPaidRequest(string? Provider, string? Reference, decimal? Amount);
