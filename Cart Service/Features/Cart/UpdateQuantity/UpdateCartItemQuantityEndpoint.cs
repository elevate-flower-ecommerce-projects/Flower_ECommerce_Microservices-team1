using System.Security.Claims;
using Cart_Service.Contracts.Cart;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Cart_Service.Features.Cart.UpdateQuantity;

public sealed class UpdateCartItemQuantityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(CartRoutes.Item, async (
            Guid itemId,
            UpdateCartItemQuantityRequest request,
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

            var result = await sender.Send(new UpdateCartItemQuantityCommand(
                userId,
                itemId,
                request.Quantity,
                request.StoreId), cancellationToken);

            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("UpdateCartItemQuantity")
        .WithTags(CartRoutes.Tag)
        .Produces<OperationResult<CartResponse>>()
        .Produces<OperationResult<StockLimitResponse>>(StatusCodes.Status409Conflict)
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden);
    }
}
