using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Contracts.Internal;
using Order___Fulfillment_Service.Entities;
using Order___Fulfillment_Service.Features.Orders;
using Order___Fulfillment_Service.Persistence;
using Repository.Layer.Interfaces;

namespace Order___Fulfillment_Service.Features.Internal.GetOrder;

public sealed class GetInternalOrderHandler(IUnitOfWork<OrderDbContext> unitOfWork)
    : IRequestHandler<GetInternalOrderQuery, OperationResult<InternalOrderResponse>>
{
    public async Task<OperationResult<InternalOrderResponse>> Handle(
        GetInternalOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<Order, Guid>()
            .Query()
            .SingleOrDefaultAsync(candidate => candidate.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return OperationResultFactory.NotFound<InternalOrderResponse>(
                message: OrderMessages.OrderNotFound,
                messageLocalized: OrderMessages.OrderNotFound);
        }

        return OperationResultFactory.Success(
            order.ToInternalResponse(),
            InternalMessages.OrderLoaded,
            InternalMessages.OrderLoaded);
    }
}
