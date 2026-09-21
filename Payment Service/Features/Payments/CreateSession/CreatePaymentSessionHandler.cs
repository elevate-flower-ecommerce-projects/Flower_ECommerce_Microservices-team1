using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payment_Service.Contracts.Payments;
using Payment_Service.Entities;
using Payment_Service.Infrastructure.Clients;
using Payment_Service.Infrastructure.Stripe;
using Payment_Service.Persistence;
using Payment_Service.Settings;
using Repository.Layer.Interfaces;

namespace Payment_Service.Features.Payments.CreateSession;

/// <summary>
/// Opens a hosted payment page for one order. The amount always comes from the order service, so a
/// client cannot decide what it pays, and an attempt row is written before Stripe is called so the
/// webhook can always find what the session belongs to.
/// </summary>
public sealed class CreatePaymentSessionHandler(
    IUnitOfWork<PaymentDbContext> unitOfWork,
    IOrderClient orderClient,
    IStripeGateway stripeGateway,
    IOptions<StripeOptions> stripeOptions,
    ILogger<CreatePaymentSessionHandler> logger)
    : IRequestHandler<CreatePaymentSessionCommand, OperationResult<object>>
{
    private readonly StripeOptions _stripeOptions = stripeOptions.Value;

    public async Task<OperationResult<object>> Handle(
        CreatePaymentSessionCommand request,
        CancellationToken cancellationToken)
    {
        if (!stripeGateway.IsConfigured)
            return Unavailable(PaymentMessages.PaymentNotConfigured, PaymentErrorCodes.PaymentUnavailable);

        var lookup = await orderClient.GetOrderAsync(request.OrderId, cancellationToken);
        if (lookup.Status is DownstreamStatus.Unavailable)
            return Unavailable(PaymentMessages.OrdersUnavailable, PaymentErrorCodes.DependencyUnavailable);

        var order = lookup.Value;

        // An order belonging to someone else is reported as missing, so this endpoint cannot be used
        // to discover which order ids exist.
        if (lookup.Status is DownstreamStatus.NotFound || order is null
            || !string.Equals(order.CustomerUserId, request.CustomerUserId, StringComparison.Ordinal))
        {
            return OperationResultFactory.NotFound<object>(
                new PaymentErrorResponse(PaymentErrorCodes.OrderNotFound),
                PaymentMessages.OrderNotFound,
                PaymentMessages.OrderNotFound);
        }

        if (order.IsPaid)
        {
            return OperationResultFactory.Conflict<object>(
                new PaymentErrorResponse(PaymentErrorCodes.OrderAlreadyPaid),
                PaymentMessages.OrderAlreadyPaid,
                PaymentMessages.OrderAlreadyPaid);
        }

        if (!order.IsPayable)
        {
            return OperationResultFactory.Conflict<object>(
                new PaymentErrorResponse(PaymentErrorCodes.OrderNotPayable),
                PaymentMessages.OrderNotPayable,
                PaymentMessages.OrderNotPayable);
        }

        var currency = _stripeOptions.Currency.ToLowerInvariant();
        if (!MoneyConversion.TryToMinorUnits(order.Total, currency, out _))
        {
            logger.LogError("Order {OrderNumber} has a total that cannot be charged: {Total}.", order.OrderNumber, order.Total);
            return OperationResultFactory.Conflict<object>(
                new PaymentErrorResponse(PaymentErrorCodes.OrderNotPayable),
                PaymentMessages.InvalidAmount,
                PaymentMessages.InvalidAmount);
        }

        var attemptRepository = unitOfWork.Repository<PaymentAttempt, Guid>();

        // Opening the payment screen twice, or coming back to it, should land on the same page
        // rather than leave a trail of abandoned sessions.
        var now = DateTime.UtcNow;
        var reusable = await attemptRepository
            .Query()
            .Where(candidate => candidate.OrderId == request.OrderId
                && candidate.Status == PaymentAttemptStatus.Pending
                && candidate.CheckoutUrl != null
                && candidate.ExpiresAtUtc != null
                && candidate.ExpiresAtUtc > now
                && candidate.Amount == order.Total
                && candidate.Currency == currency)
            .OrderByDescending(candidate => candidate.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (reusable is not null)
        {
            return OperationResultFactory.Success<object>(
                ToResponse(reusable, reused: true),
                PaymentMessages.SessionReused,
                PaymentMessages.SessionReused);
        }

        var attempt = new PaymentAttempt
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            CustomerUserId = order.CustomerUserId,
            Amount = order.Total,
            Currency = currency,
            Provider = PaymentProviders.Stripe
        };

        await attemptRepository.Create(attempt);
        await unitOfWork.CompleteAsync();

        var session = await stripeGateway.CreateCheckoutSessionAsync(attempt, cancellationToken);
        if (session.Status is not StripeCallStatus.Ok || session.Value is null)
        {
            // The attempt is closed rather than left pending, so the next try starts a clean one.
            attempt.Status = PaymentAttemptStatus.Failed;
            attempt.FailureReason = Truncate(session.Error ?? session.Status.ToString(), 500);
            attempt.UpdatedAtUtc = DateTime.UtcNow;
            attempt.CompletedAtUtc = DateTime.UtcNow;
            await attemptRepository.Update(attempt);
            await unitOfWork.CompleteAsync();

            return Unavailable(PaymentMessages.PaymentUnavailable, PaymentErrorCodes.PaymentUnavailable);
        }

        attempt.ProviderSessionId = session.Value.SessionId;
        attempt.CheckoutUrl = session.Value.Url;
        attempt.ExpiresAtUtc = session.Value.ExpiresAtUtc;
        attempt.UpdatedAtUtc = DateTime.UtcNow;
        await attemptRepository.Update(attempt);
        await unitOfWork.CompleteAsync();

        logger.LogInformation(
            "Opened payment session {SessionId} for order {OrderNumber} ({Amount} {Currency}).",
            attempt.ProviderSessionId,
            attempt.OrderNumber,
            attempt.Amount,
            attempt.Currency);

        return OperationResultFactory.Created<object>(
            ToResponse(attempt, reused: false),
            PaymentMessages.SessionCreated,
            PaymentMessages.SessionCreated);
    }

    private static PaymentSessionResponse ToResponse(PaymentAttempt attempt, bool reused)
        => new(
            attempt.Id,
            attempt.OrderId,
            attempt.OrderNumber,
            attempt.Amount,
            attempt.Currency,
            attempt.CheckoutUrl!,
            attempt.ExpiresAtUtc!.Value,
            reused);

    private static OperationResult<object> Unavailable(string message, string code)
        => OperationResultFactory.Error<object>(
            new PaymentErrorResponse(code),
            message,
            message,
            StatusCode.ServiceUnavailable);

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
