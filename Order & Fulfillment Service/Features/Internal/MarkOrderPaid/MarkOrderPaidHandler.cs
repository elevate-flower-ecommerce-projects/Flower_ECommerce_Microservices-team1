using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Internal;
using Order___Fulfillment_Service.Entities;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Order___Fulfillment_Service.Features.Internal.MarkOrderPaid;

/// <summary>
/// The only place an order becomes paid. A provider redelivers its webhook until it is acknowledged,
/// so calling this twice for the same order must succeed twice and change the order only once.
/// </summary>
public sealed class MarkOrderPaidHandler(
    IUnitOfWork<OrderDbContext> unitOfWork,
    ILogger<MarkOrderPaidHandler> logger)
    : IRequestHandler<MarkOrderPaidCommand, OperationResult<InternalOrderResponse>>
{
    public async Task<OperationResult<InternalOrderResponse>> Handle(
        MarkOrderPaidCommand request,
        CancellationToken cancellationToken)
    {
        var orderRepository = unitOfWork.Repository<Order, Guid>();
        var order = await orderRepository
            .Query(false)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return OperationResultFactory.NotFound<InternalOrderResponse>(
                message: OrderMessages.OrderNotFound,
                messageLocalized: OrderMessages.OrderNotFound);
        }

        // A redelivered confirmation lands here. Nothing to change, and it must not read as a failure.
        if (order.PaymentStatus is PaymentStatus.Paid)
        {
            return OperationResultFactory.Success(
                order.ToInternalResponse(),
                InternalMessages.OrderAlreadyPaid,
                InternalMessages.OrderAlreadyPaid);
        }

        if (order.PaymentMethod is not PaymentMethodType.Card || order.Status is OrderStatus.Cancelled)
        {
            logger.LogWarning(
                "Refused to mark order {OrderNumber} as paid: method {PaymentMethod}, status {Status}.",
                order.OrderNumber,
                order.PaymentMethod,
                order.Status);
            return OperationResultFactory.Conflict(
                order.ToInternalResponse(),
                InternalMessages.OrderNotPayable,
                InternalMessages.OrderNotPayable);
        }

        // The amount was taken from this order when the payment started, so a different amount coming
        // back means the two sides disagree; refuse rather than accept an unverified payment.
        if (request.Amount is { } paidAmount && paidAmount != order.Total)
        {
            logger.LogError(
                "Refused to mark order {OrderNumber} as paid: confirmed {PaidAmount} but the order total is {Total}.",
                order.OrderNumber,
                paidAmount,
                order.Total);
            return OperationResultFactory.Conflict(
                order.ToInternalResponse(),
                InternalMessages.OrderNotPayable,
                InternalMessages.OrderNotPayable);
        }

        order.PaymentStatus = PaymentStatus.Paid;
        if (order.Status is OrderStatus.PendingPayment)
            order.Status = OrderStatus.Placed;

        await orderRepository.Update(order);
        await unitOfWork.CompleteAsync();

        logger.LogInformation(
            "Order {OrderNumber} was marked as paid via {Provider} reference {Reference}.",
            order.OrderNumber,
            request.Provider ?? "unknown",
            request.Reference ?? "none");

        return OperationResultFactory.Success(
            order.ToInternalResponse(),
            InternalMessages.OrderMarkedPaid,
            InternalMessages.OrderMarkedPaid);
    }
}
