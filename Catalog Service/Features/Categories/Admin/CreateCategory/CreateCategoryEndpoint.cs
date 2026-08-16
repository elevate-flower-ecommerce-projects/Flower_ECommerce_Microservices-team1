using Catalog_Service.Contracts.Categories;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.Admin.CreateCategory;

public sealed class CreateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapAdminCategoryGroup()
            .MapPost("/", async (
                CreateCategoryRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new CreateCategoryCommand(
                        request.Name,
                        request.ImageUrl,
                        request.SortOrder,
                        request.IsActive),
                    cancellationToken);

                return result.ToHttpResult();
            })
            .WithName("CreateCategory")
            .WithSummary("Create a category")
            .Accepts<CreateCategoryRequest>("application/json")
            .Produces<OperationResult<AdminCategoryResponse>>(StatusCodes.Status201Created)
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<OperationResult>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
