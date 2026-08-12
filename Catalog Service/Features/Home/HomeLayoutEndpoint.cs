using Catalog_Service.Contracts.Home;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Catalog_Service.Features.Home;

public sealed class HomeLayoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/home/layout", async (
            Guid? storeId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var layout = await sender.Send(new GetHomeLayoutQuery(storeId), cancellationToken);
            return Results.Ok(layout);
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("GetHomeLayout")
        .WithTags("Home")
        .Produces<IReadOnlyList<HomeSectionResponse>>();
    }
}
