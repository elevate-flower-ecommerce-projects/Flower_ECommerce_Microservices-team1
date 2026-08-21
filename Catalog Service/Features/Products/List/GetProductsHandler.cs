using Catalog_Service.Contracts.Products;
using Catalog_Service.Entities;
using Catalog_Service.Features.Products;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Repository.Layer.Interfaces;
using System.Linq.Expressions;

namespace Catalog_Service.Features.Products.List;

public sealed class GetProductsHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<GetProductsQuery, OperationResult<PagedResponse<ProductListItemResponse>>>
{
    public async Task<OperationResult<PagedResponse<ProductListItemResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;
        var hasStoreId = request.StoreId.HasValue;
        var storeId = request.StoreId.GetValueOrDefault();
        var search = request.Search?.Trim();

        Expression<Func<Product, bool>> predicate = product =>
            product.IsActive
            && (request.CategoryId == null || product.CategoryId == request.CategoryId)
            && (request.OccasionId == null || product.OccasionId == request.OccasionId)
            && (request.StoreId == null || product.StoreInventories.Any(inventory => inventory.StoreId == request.StoreId))
            && (request.InStock == null
                || (request.InStock.Value
                    ? product.StoreInventories.Any(inventory => inventory.IsEnabled && inventory.AvailableQuantity > 0 && (!hasStoreId || inventory.StoreId == storeId))
                    : !product.StoreInventories.Any(inventory => inventory.IsEnabled && inventory.AvailableQuantity > 0 && (!hasStoreId || inventory.StoreId == storeId))))
            && (string.IsNullOrWhiteSpace(search) || product.Name.Contains(search));

        var paged = await unitOfWork.Repository<Product, Guid>()
            .GetPageSelectAsync(
                page,
                pageSize,
                predicate,
                product => new ProductListProjection(
                    product.Id,
                    product.Name,
                    product.ImageUrl,
                    product.Price,
                    product.DiscountPercent,
                    product.DiscountStartsAtUtc,
                    product.DiscountEndsAtUtc,
                    product.StoreInventories.Any(inventory => inventory.IsEnabled && inventory.AvailableQuantity > 0 && (!hasStoreId || inventory.StoreId == storeId))),
                query => query.OrderBy(product => product.Name).ThenBy(product => product.Id));

        var utcNow = DateTime.UtcNow;
        var items = paged.Items
            .Select(product =>
            {
                var price = ProductPriceCalculator.Calculate(
                    product.Price,
                    product.DiscountPercent,
                    product.DiscountStartsAtUtc,
                    product.DiscountEndsAtUtc,
                    utcNow);

                return new ProductListItemResponse(
                    product.Id,
                    product.Name,
                    product.ImageUrl,
                    price.Price,
                    price.DiscountedPrice,
                    price.DiscountPercent,
                    product.InStock);
            })
            .ToList();

        return OperationResultFactory.Success(
            new PagedResponse<ProductListItemResponse>(page, pageSize, paged.TotalCount, items));
    }

    private sealed record ProductListProjection(
        Guid Id,
        string Name,
        string? ImageUrl,
        decimal Price,
        decimal? DiscountPercent,
        DateTime? DiscountStartsAtUtc,
        DateTime? DiscountEndsAtUtc,
        bool InStock);
}
