using Catalog_Service.Contracts.Home;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

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
            return layout.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("GetHomeLayout")
        .WithTags("Home")
        .Produces<OperationResult<IReadOnlyList<HomeSectionResponse>>>();
    }
}
