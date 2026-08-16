using Catalog_Service.Contracts.Categories;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories/{categoryId:guid}", async (
            Guid categoryId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCategoryByIdQuery(categoryId), cancellationToken);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("GetCategoryById")
        .WithTags("Categories")
        .WithSummary("Get a category by id")
        .WithDescription("Returns 410 Gone when the category exists but has been archived.")
        .Produces<OperationResult<CategoryResponse>>()
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status410Gone);
    }
}
