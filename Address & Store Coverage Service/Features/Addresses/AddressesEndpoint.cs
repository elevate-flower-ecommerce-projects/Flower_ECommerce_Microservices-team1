using System.Security.Claims;
using Address___Store_Coverage_Service.Contracts.Addresses;
using Address___Store_Coverage_Service.Features.Addresses.Details;
using Address___Store_Coverage_Service.Features.Addresses.List;
using Address___Store_Coverage_Service.Features.Addresses.SetDefault;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Address___Store_Coverage_Service.Features.Addresses;

public sealed class AddressesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users/me/addresses")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
            .WithTags("Address & Coverage");

        group.MapGet("/", async (ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
        {
            var userId = ResolveUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await sender.Send(new ListMyAddressesQuery(userId), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ListMyAddresses")
        .Produces<OperationResult<IReadOnlyList<AddressListItemResponse>>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden);

        group.MapPatch("/{addressId:guid}/default", async (Guid addressId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
        {
            var userId = ResolveUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await sender.Send(new SetDefaultAddressCommand(userId, addressId), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("SetDefaultAddress")
        .Produces<OperationResult<AddressResponse>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound);

        group.MapGet("/{addressId:guid}", async (Guid addressId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
        {
            var userId = ResolveUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await sender.Send(new GetMyAddressDetailsQuery(userId, addressId), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("GetMyAddressDetails")
        .Produces<OperationResult<AddressResponse>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }

    private static string? ResolveUserId(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    private static IResult Unauthorized()
        => OperationResultFactory.UnAuthorized(
            "Missing user identity.",
            "Missing user identity.")
            .ToHttpResult();
}
