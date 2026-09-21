namespace Order___Fulfillment_Service.Contracts.Internal;

/// <summary>
/// What another Flower service needs to know about an order before charging for it. The statuses are
/// sent as names rather than numbers so a reordered enum cannot silently change their meaning, and
/// the two decisions the payment service actually makes are answered here, by the service that owns
/// the order, instead of being re-derived on the other side.
/// </summary>
public sealed record InternalOrderResponse(
    Guid OrderId,
    string OrderNumber,
    string CustomerUserId,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    decimal Total,
    bool IsPaid,
    bool IsPayable);

/// <summary>
/// Sent by the payment service once a provider has confirmed a payment. Provider and Reference are
/// recorded in the log for tracing, and Amount is checked against the order so a mismatched
/// confirmation is refused here too, not only inside the payment service.
/// </summary>
public sealed record MarkOrderPaidRequest(
    string? Provider,
    string? Reference,
    decimal? Amount);
