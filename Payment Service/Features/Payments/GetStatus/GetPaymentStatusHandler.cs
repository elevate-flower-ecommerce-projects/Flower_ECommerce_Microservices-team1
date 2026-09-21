using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment_Service.Contracts.Internal;
using Payment_Service.Contracts.Payments;
using Payment_Service.Entities;
using Payment_Service.Infrastructure.Clients;
using Payment_Service.Infrastructure.Stripe;
using Payment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Payment_Service.Features.Payments.GetStatus;

/// <summary>
/// Tells the app where a payment stands, and repairs it on the way if a webhook never arrived.
/// The customer closing the app mid-payment, or a lost webhook, both end here: we ask Stripe
/// directly and finish the job, so no paid order is left looking unpaid.
/// </summary>
public sealed class GetPaymentStatusHandler(
    IUnitOfWork<PaymentDbContext> unitOfWork,
    IOrderClient orderClient,
    IStripeGateway stripeGateway,
    IPaymentSettlement settlement,
    ILogger<GetPaymentStatusHandler> logger)
    : IRequestHandler<GetPaymentStatusQuery, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        GetPaymentStatusQuery request,
        CancellationToken cancellationToken)
    {
        var lookup = await orderClient.GetOrderAsync(request.OrderId, cancellationToken);
        if (lookup.Status is DownstreamStatus.Unavailable)
        {
            return OperationResultFactory.Error<object>(
                new PaymentErrorResponse(PaymentErrorCodes.DependencyUnavailable),
                PaymentMessages.OrdersUnavailable,
                PaymentMessages.OrdersUnavailable,
                StatusCode.ServiceUnavailable);
        }

        var order = lookup.Value;
        if (lookup.Status is DownstreamStatus.NotFound || order is null
            || !string.Equals(order.CustomerUserId, request.CustomerUserId, StringComparison.Ordinal))
        {
            return OperationResultFactory.NotFound<object>(
                new PaymentErrorResponse(PaymentErrorCodes.OrderNotFound),
                PaymentMessages.OrderNotFound,
                PaymentMessages.OrderNotFound);
        }

        var attempt = await unitOfWork.Repository<PaymentAttempt, Guid>()
            .Query(false)
            .Where(candidate => candidate.OrderId == request.OrderId)
            .OrderByDescending(candidate => candidate.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        // Nothing to reconcile once the order is paid, and nothing to ask about without a session.
        if (!order.IsPaid && attempt is { Status: PaymentAttemptStatus.Pending, ProviderSessionId: not null })
            order = await ReconcileAsync(order, attempt, cancellationToken);

        return OperationResultFactory.Success<object>(
            BuildResponse(order, attempt),
            PaymentMessages.StatusLoaded,
            PaymentMessages.StatusLoaded);
    }

    private async Task<InternalOrderSnapshot> ReconcileAsync(
        InternalOrderSnapshot order,
        PaymentAttempt attempt,
        CancellationToken cancellationToken)
    {
        var session = await stripeGateway.GetSessionAsync(attempt.ProviderSessionId!, cancellationToken);
        if (session.Status is not StripeCallStatus.Ok || session.Value is null)
            return order;

        var state = session.Value;

        if (state.IsPaid)
        {
            logger.LogInformation(
                "Reconciling order {OrderNumber}: Stripe says session {SessionId} is paid but no webhook settled it.",
                attempt.OrderNumber,
                state.SessionId);

            var outcome = await settlement.SettleAsync(
                attempt,
                state.PaymentIntentId,
                state.AmountTotalMinorUnits,
                state.Currency,
                cancellationToken);

            // Re-read the order so the caller sees the state it is actually in now.
            if (outcome is SettlementOutcome.Settled or SettlementOutcome.AlreadySettled)
            {
                var refreshed = await orderClient.GetOrderAsync(order.OrderId, cancellationToken);
                return refreshed.Value ?? order;
            }

            return order;
        }

        if (state.IsExpired)
        {
            attempt.Status = PaymentAttemptStatus.Expired;
            attempt.FailureReason = "The payment session expired.";
            attempt.CompletedAtUtc = DateTime.UtcNow;
            attempt.UpdatedAtUtc = DateTime.UtcNow;
            await unitOfWork.Repository<PaymentAttempt, Guid>().Update(attempt);
            await unitOfWork.CompleteAsync();
        }

        return order;
    }

    private static PaymentStatusResponse BuildResponse(InternalOrderSnapshot order, PaymentAttempt? attempt)
    {
        // A page is only worth offering while it is still open and has not run out of time.
        var sessionIsUsable = attempt is { Status: PaymentAttemptStatus.Pending, CheckoutUrl: not null }
            && attempt.ExpiresAtUtc > DateTime.UtcNow;

        return new PaymentStatusResponse(
            order.OrderId,
            order.OrderNumber,
            order.Status,
            order.PaymentStatus,
            order.IsPaid,
            order.Total,
            attempt?.Currency ?? string.Empty,
            attempt?.Id,
            attempt?.Status.ToString(),
            sessionIsUsable ? attempt!.CheckoutUrl : null,
            sessionIsUsable ? attempt!.ExpiresAtUtc : null,
            order.IsPayable && !sessionIsUsable);
    }
}
