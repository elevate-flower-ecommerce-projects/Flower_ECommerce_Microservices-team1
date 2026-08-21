using System.Security.Claims;
using Address___Store_Coverage_Service.Contracts.Addresses;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Address___Store_Coverage_Service.Features.Addresses.Update;

public sealed class UpdateAddressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/me/addresses/{id:guid}", async (
            Guid id,
            UpdateAddressRequest request,
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

            var result = await sender.Send(new UpdateAddressCommand(
                id,
                userId,
                request.RecipientName,
                request.Phone,
                request.AddressLine,
                request.City,
                request.Area,
                request.Lat,
                request.Lng,
                request.Label), cancellationToken);

            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("UpdateMyAddress")
        .WithTags("Address & Coverage")
        .Produces<OperationResult<AddressResponse>>(StatusCodes.Status200OK)
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden)
        .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }
}
