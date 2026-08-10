using Catalog_Service.Contracts.Home;
using Catalog_Service.Services;
using Microsoft.AspNetCore.Authorization;

namespace Catalog_Service.Features.Home;

public static class HomeLayoutEndpoint
{
    public static IEndpointRouteBuilder MapHomeLayoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/home/layout", async (
            Guid? storeId,
            IHomeLayoutService homeLayoutService,
            CancellationToken cancellationToken) =>
        {
            var layout = await homeLayoutService.GetLayoutAsync(storeId, cancellationToken);
            return Results.Ok(layout);
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("GetHomeLayout")
        .WithTags("Home")
        .Produces<IReadOnlyList<HomeSectionResponse>>();

        return app;
    }
}
