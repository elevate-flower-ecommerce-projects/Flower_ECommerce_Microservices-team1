using Catalog_Service.Contracts.Products;
using Catalog_Service.Entities;
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
        var search = request.Search?.Trim();

        Expression<Func<Product, bool>> predicate = product =>
            product.IsActive
            && (request.CategoryId == null || product.CategoryId == request.CategoryId)
            && (request.OccasionId == null || product.OccasionId == request.OccasionId)
            && (request.StoreId == null || product.StoreId == null || product.StoreId == request.StoreId)
            && (request.InStock == null || product.IsAvailable == request.InStock.Value)
            && (string.IsNullOrWhiteSpace(search) || product.Name.Contains(search));

        var paged = await unitOfWork.Repository<Product, Guid>()
            .GetPageSelectAsync(
                page,
                pageSize,
                predicate,
                product => new ProductListItemResponse(
                    product.Id,
                    product.Name,
                    product.ImageUrl,
                    product.Price,
                    product.CategoryId,
                    product.OccasionId,
                    product.IsAvailable),
                query => query.OrderBy(product => product.Name).ThenBy(product => product.Id));

        return OperationResultFactory.Success(
            new PagedResponse<ProductListItemResponse>(page, pageSize, paged.TotalCount, paged.Items));
    }
}
