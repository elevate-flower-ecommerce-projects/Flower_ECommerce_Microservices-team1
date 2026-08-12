using Catalog_Service.Contracts.Products;
using Catalog_Service.Entities;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Catalog_Service.Features.Products;

public sealed class GetProductsHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<GetProductsQuery, OperationResult<PagedResponse<ProductListItemResponse>>>
{
    public async Task<OperationResult<PagedResponse<ProductListItemResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<Product, Guid>()
            .Query()
            .Where(product => product.IsActive);

        if (request.CategoryId is not null)
            query = query.Where(product => product.CategoryId == request.CategoryId);

        if (request.OccasionId is not null)
            query = query.Where(product => product.OccasionId == request.OccasionId);

        if (request.StoreId is not null)
            query = query.Where(product => product.StoreId == null || product.StoreId == request.StoreId);

        if (request.InStock is not null)
            query = query.Where(product => product.IsAvailable == request.InStock);

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
                product.IsAvailable))
            .ToListAsync(cancellationToken);

        var utcNow = DateTime.UtcNow;
        var items = products
            .Select(product =>
            {
                var price = Calculate(
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
                    product.IsAvailable);
            })
            .ToList();

        return OperationResultFactory.Success(
            new PagedResponse<ProductListItemResponse>(page, pageSize, totalCount, items));
    }

    #region Helpers


    private static ProductPrice Calculate(decimal price, decimal? discountPercent, DateTime? discountStartsAtUtc, DateTime? discountEndsAtUtc, DateTime utcNow)
    {
        var isActive = discountPercent is > 0 and <= 100
            && (discountStartsAtUtc is null || discountStartsAtUtc <= utcNow)
            && (discountEndsAtUtc is null || discountEndsAtUtc >= utcNow);

        if (!isActive)
            return new ProductPrice(price, null, null);

        var discountedPrice = Math.Round(
            price * (1 - discountPercent!.Value / 100),
            2,
            MidpointRounding.AwayFromZero);

        return new ProductPrice(price, discountedPrice, discountPercent);
    }


    private sealed record ProductListProjection(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    decimal? DiscountPercent,
    DateTime? DiscountStartsAtUtc,
    DateTime? DiscountEndsAtUtc,
    bool IsAvailable);


    private sealed record ProductPrice(
    decimal Price,
    decimal? DiscountedPrice,
    decimal? DiscountPercent);


    #endregion

}

