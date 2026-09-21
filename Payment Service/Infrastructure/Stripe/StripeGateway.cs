using Microsoft.Extensions.Options;
using Payment_Service.Entities;
using Payment_Service.Settings;
using Stripe;
using Stripe.Checkout;

namespace Payment_Service.Infrastructure.Stripe;

public enum StripeCallStatus
{
    Ok,
    NotConfigured,
    Rejected,
    Unavailable
}

public sealed record StripeResult<T>(StripeCallStatus Status, T? Value, string? Error)
{
    public static StripeResult<T> Ok(T value) => new(StripeCallStatus.Ok, value, null);
    public static StripeResult<T> NotConfigured() => new(StripeCallStatus.NotConfigured, default, null);
    public static StripeResult<T> Rejected(string error) => new(StripeCallStatus.Rejected, default, error);
    public static StripeResult<T> Unavailable(string error) => new(StripeCallStatus.Unavailable, default, error);
}

/// <summary>A hosted checkout page Stripe is holding open for one payment attempt.</summary>
public sealed record StripeCheckoutSession(string SessionId, string Url, DateTime ExpiresAtUtc);

/// <summary>What Stripe says about a session right now, asked for rather than waited for.</summary>
public sealed record StripeSessionState(
    string SessionId,
    string Status,
    bool IsPaid,
    long? AmountTotalMinorUnits,
    string? Currency,
    string? PaymentIntentId,
    DateTime? ExpiresAtUtc)
{
    public bool IsExpired => string.Equals(Status, "expired", StringComparison.OrdinalIgnoreCase);

    public bool IsOpen => string.Equals(Status, "open", StringComparison.OrdinalIgnoreCase);
}

/// <summary>A payment event Stripe sent us, after its signature has been verified.</summary>
public sealed record StripeWebhookEvent(
    string EventId,
    string EventType,
    string? SessionId,
    string? PaymentIntentId,
    Guid? PaymentAttemptId,
    long? AmountTotalMinorUnits,
    string? Currency,
    bool IsPaid);

public interface IStripeGateway
{
    bool IsConfigured { get; }

    bool IsWebhookConfigured { get; }

    Task<StripeResult<StripeCheckoutSession>> CreateCheckoutSessionAsync(
        PaymentAttempt attempt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Asks Stripe what happened to a session. Used when a webhook has not arrived, so the customer
    /// is never left staring at a payment that already went through.
    /// </summary>
    Task<StripeResult<StripeSessionState>> GetSessionAsync(string sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies that a webhook body really came from Stripe and was not replayed, then reads the
    /// few fields we act on. Anything that fails verification is not an event as far as we care.
    /// </summary>
    bool TryReadWebhookEvent(string payload, string? signatureHeader, out StripeWebhookEvent? webhookEvent);
}

public sealed class StripeGateway : IStripeGateway
{
    // Stripe accepts an expiry between 30 minutes and 24 hours from now.
    private const int MinimumLifetimeMinutes = 30;
    private const int MaximumLifetimeMinutes = 1440;

    private readonly StripeOptions _options;
    private readonly ILogger<StripeGateway> _logger;
    private readonly IStripeClient? _client;

    public StripeGateway(IOptions<StripeOptions> options, ILogger<StripeGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = _options.IsConfigured ? new StripeClient(_options.SecretKey) : null;

        if (_client is null)
            _logger.LogWarning("Stripe is not configured: Stripe:SecretKey is empty, so online payment is disabled.");
    }

    public bool IsConfigured => _client is not null;

    public bool IsWebhookConfigured => !string.IsNullOrWhiteSpace(_options.WebhookSecret);

    public async Task<StripeResult<StripeSessionState>> GetSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (_client is null)
            return StripeResult<StripeSessionState>.NotConfigured();

        try
        {
            var session = await new SessionService(_client).GetAsync(sessionId, cancellationToken: cancellationToken);

            return StripeResult<StripeSessionState>.Ok(new StripeSessionState(
                session.Id,
                session.Status ?? string.Empty,
                string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase),
                session.AmountTotal,
                session.Currency,
                session.PaymentIntentId,
                session.ExpiresAt == default ? null : session.ExpiresAt.ToUniversalTime()));
        }
        catch (StripeException exception) when (exception.StripeError?.Type == "invalid_request_error")
        {
            _logger.LogWarning("Stripe does not know session {SessionId}.", sessionId);
            return StripeResult<StripeSessionState>.Rejected(exception.StripeError?.Message ?? exception.Message);
        }
        catch (Exception exception) when (exception is StripeException or HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            _logger.LogWarning(exception, "Stripe is unreachable while reading session {SessionId}.", sessionId);
            return StripeResult<StripeSessionState>.Unavailable(exception.Message);
        }
    }

    public bool TryReadWebhookEvent(string payload, string? signatureHeader, out StripeWebhookEvent? webhookEvent)
    {
        webhookEvent = null;

        if (!IsWebhookConfigured)
        {
            _logger.LogError("A Stripe webhook arrived but Stripe:WebhookSecret is not configured, so it cannot be verified.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        Event stripeEvent;
        try
        {
            // The signature covers the raw body and a timestamp, so a replayed or edited body fails
            // here. The API version is deliberately not enforced: Stripe may send a newer one than
            // this library knows, and the fields we read do not change between versions.
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                signatureHeader,
                _options.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException exception)
        {
            _logger.LogWarning("Rejected a Stripe webhook with an invalid signature: {Reason}.", exception.Message);
            return false;
        }

        var session = stripeEvent.Data?.Object as Session;

        webhookEvent = new StripeWebhookEvent(
            stripeEvent.Id,
            stripeEvent.Type,
            session?.Id,
            session?.PaymentIntentId,
            ReadAttemptId(session?.Metadata),
            session?.AmountTotal,
            session?.Currency,
            string.Equals(session?.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase));

        return true;
    }

    private static string WithQuery(string url, string query)
        => url + (url.Contains('?') ? "&" : "?") + query;

    private static Guid? ReadAttemptId(IDictionary<string, string>? metadata)
        => metadata is not null
            && metadata.TryGetValue("paymentAttemptId", out var value)
            && Guid.TryParse(value, out var attemptId)
                ? attemptId
                : null;

    public async Task<StripeResult<StripeCheckoutSession>> CreateCheckoutSessionAsync(
        PaymentAttempt attempt,
        CancellationToken cancellationToken)
    {
        if (_client is null)
            return StripeResult<StripeCheckoutSession>.NotConfigured();

        if (!MoneyConversion.TryToMinorUnits(attempt.Amount, attempt.Currency, out var unitAmount))
            return StripeResult<StripeCheckoutSession>.Rejected($"Amount {attempt.Amount} {attempt.Currency} cannot be charged.");

        var lifetime = Math.Clamp(_options.SessionLifetimeMinutes, MinimumLifetimeMinutes, MaximumLifetimeMinutes);

        // Carried on the session and on the payment itself, so the webhook can find this attempt
        // whichever event Stripe sends.
        var metadata = new Dictionary<string, string>
        {
            ["orderId"] = attempt.OrderId.ToString(),
            ["orderNumber"] = attempt.OrderNumber,
            ["paymentAttemptId"] = attempt.Id.ToString()
        };

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = attempt.OrderId.ToString(),
            // The order id lets the app match the return to the order it is paying. Stripe fills in
            // {CHECKOUT_SESSION_ID} itself, so it must reach Stripe unencoded.
            SuccessUrl = WithQuery(_options.SuccessUrl, $"orderId={attempt.OrderId}&sessionId={{CHECKOUT_SESSION_ID}}"),
            CancelUrl = WithQuery(_options.CancelUrl, $"orderId={attempt.OrderId}"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(lifetime),
            Metadata = metadata,
            PaymentIntentData = new SessionPaymentIntentDataOptions { Metadata = metadata },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = attempt.Currency,
                        UnitAmount = unitAmount,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Flower order {attempt.OrderNumber}"
                        }
                    }
                }
            ]
        };

        // If this call is retried after a timeout, Stripe returns the session it already created
        // instead of opening a second one for the same attempt.
        var requestOptions = new RequestOptions { IdempotencyKey = $"session_{attempt.Id}" };

        try
        {
            var session = await new SessionService(_client).CreateAsync(options, requestOptions, cancellationToken);

            if (string.IsNullOrWhiteSpace(session.Url))
                return StripeResult<StripeCheckoutSession>.Unavailable("Stripe returned a session without a URL.");

            return StripeResult<StripeCheckoutSession>.Ok(new StripeCheckoutSession(
                session.Id,
                session.Url,
                session.ExpiresAt == default ? DateTime.UtcNow.AddMinutes(lifetime) : session.ExpiresAt.ToUniversalTime()));
        }
        catch (StripeException exception) when (exception.StripeError?.Type == "invalid_request_error")
        {
            // Our request was wrong, so retrying it unchanged would fail the same way.
            _logger.LogError(exception, "Stripe rejected the checkout session for attempt {AttemptId}.", attempt.Id);
            return StripeResult<StripeCheckoutSession>.Rejected(exception.StripeError?.Message ?? exception.Message);
        }
        catch (Exception exception) when (exception is StripeException or HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            _logger.LogWarning(exception, "Stripe is unreachable while creating a session for attempt {AttemptId}.", attempt.Id);
            return StripeResult<StripeCheckoutSession>.Unavailable(exception.Message);
        }
    }
}
