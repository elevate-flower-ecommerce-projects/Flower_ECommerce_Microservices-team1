using Catalog_Service.Contracts.Categories;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.GetCategories;

public sealed class GetCategoriesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (
            string? search,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCategoriesQuery(search), cancellationToken);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("GetCategories")
        .WithTags("Categories")
        .WithSummary("List active categories")
        .Produces<OperationResult<IReadOnlyList<CategoryResponse>>>();
    }
}
