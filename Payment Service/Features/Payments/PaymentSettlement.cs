using Payment_Service.Entities;
using Payment_Service.Infrastructure.Clients;
using Payment_Service.Infrastructure.Stripe;
using Payment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Payment_Service.Features.Payments;

public enum SettlementOutcome
{
    /// <summary>The attempt succeeded and the order service has confirmed it.</summary>
    Settled,

    /// <summary>Nothing left to do: this attempt was already settled and the order already told.</summary>
    AlreadySettled,

    /// <summary>What the provider charged is not what this attempt is for. The order stays unpaid.</summary>
    AmountMismatch,

    /// <summary>The order service answered, and refused. Retrying will not change that.</summary>
    Refused,

    /// <summary>The order service could not be reached. Worth trying again.</summary>
    NotifyFailed
}

public interface IPaymentSettlement
{
    Task<SettlementOutcome> SettleAsync(
        PaymentAttempt attempt,
        string? paymentIntentId,
        long? chargedMinorUnits,
        string? chargedCurrency,
        CancellationToken cancellationToken);
}

/// <summary>
/// Turns a confirmed payment into a paid order. Both the webhook and the status endpoint go through
/// here, so the two routes into "paid" behave identically and can each be repeated safely.
/// </summary>
public sealed class PaymentSettlement(
    IUnitOfWork<PaymentDbContext> unitOfWork,
    IOrderClient orderClient,
    ICartClient cartClient,
    ILogger<PaymentSettlement> logger) : IPaymentSettlement
{
    public async Task<SettlementOutcome> SettleAsync(
        PaymentAttempt attempt,
        string? paymentIntentId,
        long? chargedMinorUnits,
        string? chargedCurrency,
        CancellationToken cancellationToken)
    {
        if (attempt.Status is PaymentAttemptStatus.Succeeded && attempt.OrderNotifiedAtUtc is not null)
            return SettlementOutcome.AlreadySettled;

        // The amount was taken from the order when the attempt was created. If what the provider
        // charged differs, something is wrong and this is not a payment for this order.
        if (chargedMinorUnits is not null || chargedCurrency is not null)
        {
            MoneyConversion.TryToMinorUnits(attempt.Amount, attempt.Currency, out var expected);
            var currencyMatches = string.Equals(chargedCurrency, attempt.Currency, StringComparison.OrdinalIgnoreCase);

            if (chargedMinorUnits != expected || !currencyMatches)
            {
                logger.LogError(
                    "Payment attempt {AttemptId} for order {OrderNumber} expected {Expected} {Currency} but the provider reported {Charged} {ChargedCurrency}.",
                    attempt.Id,
                    attempt.OrderNumber,
                    expected,
                    attempt.Currency,
                    chargedMinorUnits,
                    chargedCurrency);

                await FailAttemptAsync(attempt, "Provider reported a different amount or currency.", cancellationToken);
                return SettlementOutcome.AmountMismatch;
            }
        }

        var attemptRepository = unitOfWork.Repository<PaymentAttempt, Guid>();

        if (attempt.Status is not PaymentAttemptStatus.Succeeded)
        {
            attempt.Status = PaymentAttemptStatus.Succeeded;
            attempt.ProviderPaymentIntentId ??= paymentIntentId;
            attempt.CompletedAtUtc ??= DateTime.UtcNow;
            attempt.UpdatedAtUtc = DateTime.UtcNow;
            await attemptRepository.Update(attempt);
            await unitOfWork.CompleteAsync();
        }

        var outcome = await orderClient.MarkOrderPaidAsync(
            attempt.OrderId,
            attempt.Amount,
            attempt.Provider,
            paymentIntentId ?? attempt.ProviderPaymentIntentId ?? attempt.ProviderSessionId ?? attempt.Id.ToString(),
            cancellationToken);

        switch (outcome)
        {
            case MarkOrderPaidOutcome.Confirmed:
                attempt.OrderNotifiedAtUtc = DateTime.UtcNow;
                attempt.UpdatedAtUtc = DateTime.UtcNow;
                await attemptRepository.Update(attempt);
                await unitOfWork.CompleteAsync();

                // The order is already paid and recorded, so a cart that survives is a nuisance we
                // log rather than a reason to make the provider send the payment again.
                if (!await cartClient.ClearCartAsync(attempt.CustomerUserId, cancellationToken))
                    logger.LogWarning("Order {OrderNumber} is paid but its cart was not cleared.", attempt.OrderNumber);

                logger.LogInformation("Order {OrderNumber} is paid and confirmed.", attempt.OrderNumber);
                return SettlementOutcome.Settled;

            case MarkOrderPaidOutcome.NotFound:
            case MarkOrderPaidOutcome.Refused:
                logger.LogError(
                    "The order service refused payment {AttemptId} for order {OrderNumber}. The money was taken and needs a manual review.",
                    attempt.Id,
                    attempt.OrderNumber);
                return SettlementOutcome.Refused;

            default:
                logger.LogWarning(
                    "Could not reach the order service to confirm payment {AttemptId}; it will be retried.",
                    attempt.Id);
                return SettlementOutcome.NotifyFailed;
        }
    }

    private async Task FailAttemptAsync(PaymentAttempt attempt, string reason, CancellationToken cancellationToken)
    {
        if (attempt.Status is not PaymentAttemptStatus.Pending)
            return;

        attempt.Status = PaymentAttemptStatus.Failed;
        attempt.FailureReason = reason;
        attempt.CompletedAtUtc = DateTime.UtcNow;
        attempt.UpdatedAtUtc = DateTime.UtcNow;
        await unitOfWork.Repository<PaymentAttempt, Guid>().Update(attempt);
        await unitOfWork.CompleteAsync();
    }
}
