using Catalog_Service.Contracts.Categories;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.Admin.SetCategoryActive;

public sealed class SetCategoryActiveEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapAdminCategoryGroup();

        group.MapPatch("/{categoryId:guid}/archive", async (
            Guid categoryId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new SetCategoryActiveCommand(categoryId, IsActive: false),
                cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("ArchiveCategory")
        .WithSummary("Archive a category")
        .WithDescription("Hides the category from customers. Existing deep links then answer 410 Gone.")
        .Produces<OperationResult<AdminCategoryResponse>>()
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPatch("/{categoryId:guid}/restore", async (
            Guid categoryId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new SetCategoryActiveCommand(categoryId, IsActive: true),
                cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("RestoreCategory")
        .WithSummary("Restore an archived category")
        .Produces<OperationResult<AdminCategoryResponse>>()
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
