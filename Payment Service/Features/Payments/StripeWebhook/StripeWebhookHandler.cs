using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment_Service.Entities;
using Payment_Service.Infrastructure.Stripe;
using Payment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Payment_Service.Features.Payments.StripeWebhook;

public static class StripeEventTypes
{
    public const string CheckoutSessionCompleted = "checkout.session.completed";
    public const string CheckoutSessionAsyncPaymentSucceeded = "checkout.session.async_payment_succeeded";
    public const string CheckoutSessionAsyncPaymentFailed = "checkout.session.async_payment_failed";
    public const string CheckoutSessionExpired = "checkout.session.expired";
}

/// <summary>
/// The only place a payment is believed. Stripe retries an event until it is acknowledged, so the
/// same event arrives more than once; every step here is written to survive that.
/// </summary>
public sealed class StripeWebhookHandler(
    IUnitOfWork<PaymentDbContext> unitOfWork,
    IStripeGateway stripeGateway,
    IPaymentSettlement settlement,
    ILogger<StripeWebhookHandler> logger)
    : IRequestHandler<StripeWebhookCommand, WebhookResult>
{
    public async Task<WebhookResult> Handle(StripeWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!stripeGateway.TryReadWebhookEvent(request.Payload, request.SignatureHeader, out var stripeEvent)
            || stripeEvent is null)
        {
            return WebhookResult.Rejected;
        }

        var processedRepository = unitOfWork.Repository<ProcessedPaymentEvent, Guid>();
        var alreadyProcessed = await processedRepository
            .Query()
            .AnyAsync(
                processed => processed.Provider == PaymentProviders.Stripe && processed.EventId == stripeEvent.EventId,
                cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Stripe event {EventId} was already handled; ignoring the redelivery.", stripeEvent.EventId);
            return WebhookResult.Acknowledged;
        }

        var attempt = await FindAttemptAsync(stripeEvent, cancellationToken);
        if (attempt is null)
        {
            // Sessions created outside this service, for example by a test trigger, are not ours.
            logger.LogWarning(
                "Stripe event {EventId} ({EventType}) does not match any payment attempt here.",
                stripeEvent.EventId,
                stripeEvent.EventType);
            await RecordProcessedAsync(stripeEvent, null, cancellationToken);
            return WebhookResult.Acknowledged;
        }

        var handled = stripeEvent.EventType switch
        {
            StripeEventTypes.CheckoutSessionCompleted or StripeEventTypes.CheckoutSessionAsyncPaymentSucceeded
                => await HandlePaidAsync(stripeEvent, attempt, cancellationToken),

            StripeEventTypes.CheckoutSessionAsyncPaymentFailed
                => await CloseAttemptAsync(attempt, PaymentAttemptStatus.Failed, "The payment failed at Stripe.", cancellationToken),

            StripeEventTypes.CheckoutSessionExpired
                => await CloseAttemptAsync(attempt, PaymentAttemptStatus.Expired, "The payment session expired.", cancellationToken),

            _ => WebhookResult.Acknowledged
        };

        // Only a finished event is written down. If confirming the order failed, this stays unwritten
        // so the next delivery tries again instead of being dropped as a duplicate.
        if (handled is WebhookResult.Acknowledged)
            await RecordProcessedAsync(stripeEvent, attempt.Id, cancellationToken);

        return handled;
    }

    private async Task<WebhookResult> HandlePaidAsync(
        StripeWebhookEvent stripeEvent,
        PaymentAttempt attempt,
        CancellationToken cancellationToken)
    {
        // A completed session can still be waiting on a slow payment method; that is not paid yet.
        if (!stripeEvent.IsPaid)
        {
            logger.LogInformation(
                "Stripe session {SessionId} completed but is not paid yet; leaving attempt {AttemptId} open.",
                stripeEvent.SessionId,
                attempt.Id);
            return WebhookResult.Acknowledged;
        }

        var outcome = await settlement.SettleAsync(
            attempt,
            stripeEvent.PaymentIntentId,
            stripeEvent.AmountTotalMinorUnits,
            stripeEvent.Currency,
            cancellationToken);

        // Only an unreachable order service is worth another delivery. A mismatch or a refusal is an
        // answer, and repeating it would just replay the same failure forever.
        return outcome is SettlementOutcome.NotifyFailed ? WebhookResult.Retry : WebhookResult.Acknowledged;
    }

    private async Task<WebhookResult> CloseAttemptAsync(
        PaymentAttempt attempt,
        PaymentAttemptStatus status,
        string reason,
        CancellationToken cancellationToken)
    {
        // A paid attempt is final. A late failure or expiry event must never undo it.
        if (attempt.Status is not PaymentAttemptStatus.Pending)
            return WebhookResult.Acknowledged;

        attempt.Status = status;
        attempt.FailureReason = reason;
        attempt.CompletedAtUtc = DateTime.UtcNow;
        attempt.UpdatedAtUtc = DateTime.UtcNow;
        await unitOfWork.Repository<PaymentAttempt, Guid>().Update(attempt);
        await unitOfWork.CompleteAsync();

        logger.LogInformation("Payment attempt {AttemptId} for order {OrderNumber} ended as {Status}.", attempt.Id, attempt.OrderNumber, status);
        return WebhookResult.Acknowledged;
    }

    private async Task<PaymentAttempt?> FindAttemptAsync(StripeWebhookEvent stripeEvent, CancellationToken cancellationToken)
    {
        var attempts = unitOfWork.Repository<PaymentAttempt, Guid>();

        if (stripeEvent.PaymentAttemptId is { } attemptId)
        {
            var byMetadata = await attempts.Query(false).SingleOrDefaultAsync(candidate => candidate.Id == attemptId, cancellationToken);
            if (byMetadata is not null)
                return byMetadata;
        }

        if (!string.IsNullOrWhiteSpace(stripeEvent.SessionId))
        {
            return await attempts
                .Query(false)
                .SingleOrDefaultAsync(candidate => candidate.ProviderSessionId == stripeEvent.SessionId, cancellationToken);
        }

        return null;
    }

    private async Task RecordProcessedAsync(StripeWebhookEvent stripeEvent, Guid? attemptId, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.Repository<ProcessedPaymentEvent, Guid>().Create(new ProcessedPaymentEvent
            {
                Provider = PaymentProviders.Stripe,
                EventId = stripeEvent.EventId,
                EventType = stripeEvent.EventType,
                PaymentAttemptId = attemptId
            });
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException)
        {
            // Two deliveries of the same event raced. The unique index settled it; nothing to do.
            logger.LogInformation("Stripe event {EventId} was recorded by a concurrent delivery.", stripeEvent.EventId);
        }
    }
}
