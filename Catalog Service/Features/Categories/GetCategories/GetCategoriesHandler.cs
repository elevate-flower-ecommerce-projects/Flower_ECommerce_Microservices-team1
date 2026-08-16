using Catalog_Service.Contracts.Categories;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Categories.GetCategories;

/// <summary>
/// Returns the active category bar. Reads straight from the database so
/// administrator changes appear without a client release.
/// </summary>
public sealed class GetCategoriesHandler(CatalogDbContext dbContext)
    : IRequestHandler<GetCategoriesQuery, OperationResult<IReadOnlyList<CategoryResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<CategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(category => category.Name.Contains(search));
        }

        var categories = await query
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.ImageUrl,
                CategoryDeepLink.For(category.Id)))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<CategoryResponse>>(categories);
    }
}
