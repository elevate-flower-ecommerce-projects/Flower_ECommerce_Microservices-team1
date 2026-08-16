using Catalog_Service.Contracts.Products;
using Catalog_Service.Entities;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Catalog_Service.Features.Products.Details;

public sealed class GetProductDetailsHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<GetProductDetailsQuery, OperationResult<ProductDetailResponse>>
{
    public async Task<OperationResult<ProductDetailResponse>> Handle(
        GetProductDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<Product, Guid>()
            .Query()
            .Where(product => product.Id == request.ProductId && product.IsActive);

        if (request.StoreId is not null)
        {
            query = query.Where(product => product.StoreId == null || product.StoreId == request.StoreId);
        }

        var product = await query
            .Select(product => new ProductDetailResponse(
                product.Id,
                product.Name,
                product.ImageUrl,
                product.Price,
                product.CategoryId,
                product.OccasionId,
                product.StoreId,
                product.IsAvailable,
                product.SoldCount))
            .SingleOrDefaultAsync(cancellationToken);

        return product is null
            ? OperationResultFactory.NotFound<ProductDetailResponse>(message: "Product was not found.")
            : OperationResultFactory.Success(product);
    }
}
