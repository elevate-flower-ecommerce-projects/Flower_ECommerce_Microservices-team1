using Catalog_Service.Contracts.Categories;
using Catalog_Service.Entities;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Catalog_Service.Features.Categories.GetCategories;

/// <summary>
/// Returns the active category bar. Reads through the shared repository so
/// administrator changes appear without a client release.
/// </summary>
public sealed class GetCategoriesHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<GetCategoriesQuery, OperationResult<IReadOnlyList<CategoryResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<CategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.Repository<Category, Guid>()
            .Query()
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.ImageUrl,
                CategoryDeepLink.For(category.Id)))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<CategoryResponse>>(categories);
    }
}