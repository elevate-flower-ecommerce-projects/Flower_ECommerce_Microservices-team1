using Catalog_Service.Contracts.Products;
using Catalog_Service.Entities;
using Catalog_Service.Features.Products;
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
        var product = await unitOfWork.Repository<Product, Guid>()
            .Query()
            .Include(product => product.Images)
            .Include(product => product.IncludedItems)
            .Include(product => product.StoreInventories)
            .SingleOrDefaultAsync(
                product => product.Id == request.ProductId && product.IsActive,
                cancellationToken);

        if (product is null)
            return OperationResultFactory.NotFound<ProductDetailResponse>(message: "Product was not found.");

        var inventory = request.StoreId is null
            ? null
            : product.StoreInventories.SingleOrDefault(item => item.StoreId == request.StoreId);

        var requiresStoreSelection = request.StoreId is null;
        var availableQuantity = inventory?.AvailableQuantity ?? 0;
        var inStock = inventory?.IsEnabled == true && availableQuantity > 0;
        var price = ProductPriceCalculator.Calculate(
            product.Price,
            product.DiscountPercent,
            product.DiscountStartsAtUtc,
            product.DiscountEndsAtUtc,
            DateTime.UtcNow);

        return OperationResultFactory.Success(new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Images.OrderBy(image => image.SortOrder).Select(image => image.ImageUrl).ToList(),
            product.IncludedItems
                .OrderBy(item => item.SortOrder)
                .Select(item => new ProductIncludedItemResponse(item.Name, item.Quantity))
                .ToList(),
            price.Price,
            price.DiscountedPrice,
            price.DiscountPercent,
            requiresStoreSelection,
            inStock,
            availableQuantity));
    }
}
