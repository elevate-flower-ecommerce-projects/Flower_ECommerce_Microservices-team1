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
        var query = unitOfWork.Repository<Category, Guid>()
            .Query()
            .Where(category => category.IsActive);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(category => category.Name.Contains(search));
        }

        var products = unitOfWork.Repository<Product, Guid>().Query();

        var categories = await ApplySorting(query, products, request.SortBy)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.ImageUrl,
                CategoryDeepLink.For(category.Id)))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<CategoryResponse>>(categories);
    }

    private static IOrderedQueryable<Category> ApplySorting(
        IQueryable<Category> query,
        IQueryable<Product> products,
        CategorySortBy? sortBy) => sortBy switch
    {
        CategorySortBy.PriceAsc => query
            .OrderBy(category => products
                .Where(product => product.IsActive && product.CategoryId == category.Id)
                .Select(product => (decimal?)product.Price)
                .Min() ?? decimal.MaxValue)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id),

        CategorySortBy.PriceDesc => query
            .OrderByDescending(category => products
                .Where(product => product.IsActive && product.CategoryId == category.Id)
                .Select(product => (decimal?)product.Price)
                .Max() ?? 0m)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id),

        CategorySortBy.Newest => query
            .OrderByDescending(category => products
                .Where(product => product.IsActive && product.CategoryId == category.Id)
                .Select(product => (DateTime?)product.CreatedAtUtc)
                .Max() ?? DateTime.MinValue)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id),

        CategorySortBy.Oldest => query
            .OrderBy(category => products
                .Where(product => product.IsActive && product.CategoryId == category.Id)
                .Select(product => (DateTime?)product.CreatedAtUtc)
                .Min() ?? DateTime.MaxValue)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id),

        CategorySortBy.Discount => query
            .OrderByDescending(category => products
                .Where(product => product.IsActive && product.CategoryId == category.Id)
                .Select(product => product.DiscountPercent)
                .Max() ?? 0m)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id),

        _ => query
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id)
    };
}
