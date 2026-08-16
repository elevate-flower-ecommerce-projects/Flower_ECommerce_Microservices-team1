using Catalog_Service.Contracts.Products;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Products.Details;

public sealed record GetProductDetailsQuery(Guid ProductId, Guid? StoreId)
    : IRequest<OperationResult<ProductDetailResponse>>;
