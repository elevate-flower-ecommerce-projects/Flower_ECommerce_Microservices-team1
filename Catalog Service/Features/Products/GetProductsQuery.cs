using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Products;

public sealed record GetProductsQuery(int Page, int PageSize, Guid? OccasionId)
    : IRequest<OperationResult<PagedResponse<ProductSummaryResponse>>>;

public sealed class GetProductsQueryHandler(CatalogDbContext dbContext)
    : IRequestHandler<GetProductsQuery, OperationResult<PagedResponse<ProductSummaryResponse>>>
{
    public async Task<OperationResult<PagedResponse<ProductSummaryResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;

        var products = dbContext.Products
            .AsNoTracking()
            .Where(product => !product.IsArchived);

        if (request.OccasionId is { } occasionId)
        {
            products = products.Where(product => product.ProductOccasions.Any(productOccasion =>
                productOccasion.OccasionId == occasionId && !productOccasion.Occasion.IsArchived));
        }

        var totalCount = await products.CountAsync(cancellationToken);
        var items = await products
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new ProductSummaryResponse(
                product.Id,
                product.Name,
                product.ImageUrl,
                product.Price,
                product.InStock))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success(new PagedResponse<ProductSummaryResponse>(
            page,
            pageSize,
            totalCount,
            items));
    }
}
