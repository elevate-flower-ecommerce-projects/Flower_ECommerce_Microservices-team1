using System.Security.Claims;
using Cart_Service.Contracts.Cart;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Cart_Service.Features.Cart.AddItem;

public sealed class AddCartItemEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(CartRoutes.Items, async (
            AddCartItemRequest request,
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

            var result = await sender.Send(new AddCartItemCommand(
                userId,
                request.ProductId,
                request.Quantity,
                request.StoreId), cancellationToken);

            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("AddCartItem")
        .WithTags(CartRoutes.Tag)
        .Produces<OperationResult<CartResponse>>(StatusCodes.Status201Created)
        .Produces<OperationResult<StockLimitResponse>>(StatusCodes.Status409Conflict)
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden);
    }
}
