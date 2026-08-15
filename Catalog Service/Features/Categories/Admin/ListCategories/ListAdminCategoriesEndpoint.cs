using Catalog_Service.Contracts.Categories;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.Admin.ListCategories;

public sealed class ListAdminCategoriesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapAdminCategoryGroup()
            .MapGet("/", async (
                bool? includeArchived,
                string? search,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new ListAdminCategoriesQuery(includeArchived ?? true, search),
                    cancellationToken);

                return result.ToHttpResult();
            })
            .WithName("ListAdminCategories")
            .WithSummary("List categories for administration")
            .WithDescription("Includes archived categories by default so they can be restored.")
            .Produces<OperationResult<IReadOnlyList<AdminCategoryResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
