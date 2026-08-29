using System.Security.Claims;
using Address___Store_Coverage_Service.Contracts.Addresses;
using Address___Store_Coverage_Service.Features.Addresses.SetDefault;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Address___Store_Coverage_Service.Features.Addresses.SetDefault;

public sealed class SetDefaultAddressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/users/me/addresses/{id:guid}/default", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResultFactory.UnAuthorized(
                    "Missing user identity.",
                    "Missing user identity.")
                    .ToHttpResult();
            }

            var result = await sender.Send(new SetDefaultAddressCommand(userId, id), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("SetDefaultAddress")
        .WithTags("Address & Coverage")
        .Produces<OperationResult<AddressResponse>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }
}
