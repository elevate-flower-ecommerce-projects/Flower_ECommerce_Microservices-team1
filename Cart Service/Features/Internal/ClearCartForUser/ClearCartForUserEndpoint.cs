using Carter;
using Cart_Service.Features.Cart;
using Cart_Service.Features.Cart.ClearCart;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Cart_Service.Features.Internal.ClearCartForUser;

/// <summary>
/// Clears a named customer's cart on behalf of another service. It reuses the same handler as the
/// customer-facing endpoint, so clearing an already empty cart is still a success and the call can
/// be retried safely.
/// </summary>
public sealed class ClearCartForUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(InternalRoutes.ClearUserCart, async (
            string userId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResultFactory
                    .BadRequest(InternalMessages.UserIdRequired, InternalMessages.UserIdRequired)
                    .ToHttpResult();
            }

            var result = await sender.Send(new ClearCartCommand(userId), cancellationToken);
            return result.ToHttpResult();
        })
        .AddEndpointFilter<InternalOnlyFilter>()
        .WithName("ClearCartForUser")
        .ExcludeFromDescription()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<OperationResult>(StatusCodes.Status400BadRequest)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized);
    }
}
