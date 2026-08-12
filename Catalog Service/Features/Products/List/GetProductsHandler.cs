using Catalog_Service.Contracts.Products;
using Catalog_Service.Entities;
using Catalog_Service.Features.Products;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Catalog_Service.Features.Products.List;

public sealed class GetProductsHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<GetProductsQuery, OperationResult<PagedResponse<ProductListItemResponse>>>
{
    public async Task<OperationResult<PagedResponse<ProductListItemResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;
        var hasStoreId = request.StoreId.HasValue;
        var storeId = request.StoreId.GetValueOrDefault();

        var query = unitOfWork.Repository<Product, Guid>()
            .Query()
            .Where(product => product.IsActive);

        if (request.CategoryId is not null)
            query = query.Where(product => product.CategoryId == request.CategoryId);

        if (request.OccasionId is not null)
            query = query.Where(product => product.OccasionId == request.OccasionId);

        if (request.StoreId is not null)
        {
            query = query.Where(product => product.StoreInventories
                .Any(inventory => inventory.StoreId == request.StoreId));
        }

        if (request.InStock is not null)
        {
            query = request.InStock.Value
                ? query.Where(product => product.StoreInventories.Any(inventory =>
                    inventory.IsEnabled
                    && inventory.AvailableQuantity > 0
                    && (!hasStoreId || inventory.StoreId == storeId)))
                : query.Where(product => !product.StoreInventories.Any(inventory =>
                    inventory.IsEnabled
                    && inventory.AvailableQuantity > 0
                    && (!hasStoreId || inventory.StoreId == storeId)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new ProductListProjection(
                product.Id,
                product.Name,
                product.ImageUrl,
                product.Price,
                product.DiscountPercent,
                product.DiscountStartsAtUtc,
                product.DiscountEndsAtUtc,
                product.StoreInventories.Any(inventory =>
                    inventory.IsEnabled
                    && inventory.AvailableQuantity > 0
                    && (!hasStoreId || inventory.StoreId == storeId))))
            .ToListAsync(cancellationToken);

        var utcNow = DateTime.UtcNow;
        var items = products
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
            new PagedResponse<ProductListItemResponse>(page, pageSize, totalCount, items));
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
