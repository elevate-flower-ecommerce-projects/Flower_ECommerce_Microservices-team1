using Catalog_Service.Contracts.Categories;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Categories.Admin.ListCategories;

/// <summary>
/// Administrator view of the categories, including archived ones so they can be restored.
/// </summary>
public sealed class ListAdminCategoriesHandler(CatalogDbContext dbContext)
    : IRequestHandler<ListAdminCategoriesQuery, OperationResult<IReadOnlyList<AdminCategoryResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<AdminCategoryResponse>>> Handle(
        ListAdminCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Categories.AsNoTracking();

        if (!request.IncludeArchived)
        {
            query = query.Where(category => category.IsActive);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(category => category.Name.Contains(search));
        }

        var categories = await query
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new AdminCategoryResponse(
                category.Id,
                category.Name,
                category.ImageUrl,
                category.SortOrder,
                category.IsActive))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<AdminCategoryResponse>>(categories);
    }
}
