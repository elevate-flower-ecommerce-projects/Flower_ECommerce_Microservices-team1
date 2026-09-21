using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Internal;

namespace Order___Fulfillment_Service.Features.Internal.MarkOrderPaid;

public sealed class MarkOrderPaidEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(InternalRoutes.OrderPaymentSucceeded, async (
            Guid orderId,
            MarkOrderPaidRequest? request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkOrderPaidCommand(
                orderId,
                request?.Provider,
                request?.Reference,
                request?.Amount);

            var result = await sender.Send(command, cancellationToken);
            return result.ToHttpResult();
        })
        .AddEndpointFilter<InternalOnlyFilter>()
        .WithName("MarkOrderPaid")
        .ExcludeFromDescription()
        .Produces<OperationResult<InternalOrderResponse>>(StatusCodes.Status200OK)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult<InternalOrderResponse>>(StatusCodes.Status409Conflict);
    }
}
