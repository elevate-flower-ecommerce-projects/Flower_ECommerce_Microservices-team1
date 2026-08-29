using Catalog_Service.Contracts.Home;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Home;

public sealed class HomeLayoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/home/layout", HandleAsync)
            .AllowAnonymous()
            .WithName("GetHomeLayout")
            .WithTags("Home")
            .Produces<OperationResult<IReadOnlyList<HomeSectionResponse>>>();

        app.MapGet("/home/sections", HandleAsync)
            .AllowAnonymous()
            .WithName("GetHomeSections")
            .WithTags("Home")
            .Produces<OperationResult<IReadOnlyList<HomeSectionResponse>>>();
    }

    private static async Task<IResult> HandleAsync(
        Guid? storeId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var layout = await sender.Send(new GetHomeLayoutQuery(storeId), cancellationToken);
        return layout.ToHttpResult();
    }
}
