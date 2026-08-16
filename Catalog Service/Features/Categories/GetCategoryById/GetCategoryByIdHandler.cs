using Catalog_Service.Contracts.Categories;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Categories.GetCategoryById;

/// <summary>
/// Resolves a single category for deep links.
/// An archived category answers 410 Gone instead of 404 so an old link can render a
/// "no longer available" screen rather than a dead end.
/// </summary>
public sealed class GetCategoryByIdHandler(CatalogDbContext dbContext)
    : IRequestHandler<GetCategoryByIdQuery, OperationResult<CategoryResponse>>
{
    internal const string ArchivedMessage = "This category is no longer available.";
    internal const string NotFoundMessage = "Category was not found.";

    public async Task<OperationResult<CategoryResponse>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Archived rows are loaded on purpose: the two outcomes must stay distinguishable.
        var category = await dbContext.Categories
            .AsNoTracking()
            .Where(entity => entity.Id == request.CategoryId)
            .Select(entity => new
            {
                entity.Id,
                entity.Name,
                entity.ImageUrl,
                entity.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
            return OperationResultFactory.NotFound<CategoryResponse>(
                message: NotFoundMessage,
                messageLocalized: NotFoundMessage);

        if (!category.IsActive)
            return OperationResultFactory.Error<CategoryResponse>(
                message: ArchivedMessage,
                messageLocalized: ArchivedMessage,
                statusCode: StatusCode.Gone);

        return OperationResultFactory.Success(new CategoryResponse(
            category.Id,
            category.Name,
            category.ImageUrl,
            CategoryDeepLink.For(category.Id)));
    }
}
