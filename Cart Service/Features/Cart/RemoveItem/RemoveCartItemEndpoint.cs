using System.Security.Claims;
using Cart_Service.Contracts.Cart;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Cart_Service.Features.Cart.RemoveItem;

public sealed class RemoveCartItemEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(CartRoutes.Item, async (
            Guid itemId,
            Guid? storeId,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResultFactory.UnAuthorized(
                    CartMessages.MissingIdentity,
                    CartMessages.MissingIdentity)
                    .ToHttpResult();
            }

            var result = await sender.Send(new RemoveCartItemCommand(userId, itemId, storeId), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("RemoveCartItem")
        .WithTags(CartRoutes.Tag)
        .Produces<OperationResult<CartResponse>>()
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden);
    }
}
