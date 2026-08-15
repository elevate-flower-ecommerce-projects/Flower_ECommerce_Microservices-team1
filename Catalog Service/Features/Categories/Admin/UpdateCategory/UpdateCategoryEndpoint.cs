using Catalog_Service.Contracts.Categories;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.Admin.UpdateCategory;

public sealed class UpdateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapAdminCategoryGroup()
            .MapPut("/{categoryId:guid}", async (
                Guid categoryId,
                UpdateCategoryRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new UpdateCategoryCommand(
                        categoryId,
                        request.Name,
                        request.ImageUrl,
                        request.SortOrder),
                    cancellationToken);

                return result.ToHttpResult();
            })
            .WithName("UpdateCategory")
            .WithSummary("Rename or restyle a category")
            .Accepts<UpdateCategoryRequest>("application/json")
            .Produces<OperationResult<AdminCategoryResponse>>()
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<OperationResult>(StatusCodes.Status404NotFound)
            .Produces<OperationResult>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
