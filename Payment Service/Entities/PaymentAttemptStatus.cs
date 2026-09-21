namespace Payment_Service.Entities;

/// <summary>
/// The life of a single attempt to pay one order. Succeeded, Failed, Canceled and Expired are
/// final: once an attempt reaches any of them, a later provider event must not move it again.
/// A failed attempt does not fail the order; the customer can start a new attempt for it.
/// </summary>
public enum PaymentAttemptStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Canceled = 4,
    Expired = 5
}
