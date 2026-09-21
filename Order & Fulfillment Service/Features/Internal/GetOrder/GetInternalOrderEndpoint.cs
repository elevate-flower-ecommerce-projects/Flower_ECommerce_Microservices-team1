using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Internal;

namespace Order___Fulfillment_Service.Features.Internal.GetOrder;

public sealed class GetInternalOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(InternalRoutes.Order, async (
            Guid orderId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetInternalOrderQuery(orderId), cancellationToken);
            return result.ToHttpResult();
        })
        .AddEndpointFilter<InternalOnlyFilter>()
        .WithName("GetInternalOrder")
        .ExcludeFromDescription()
        .Produces<OperationResult<InternalOrderResponse>>(StatusCodes.Status200OK)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }
}
