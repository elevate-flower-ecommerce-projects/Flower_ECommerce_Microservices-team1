using Catalog_Service.Contracts.Products;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Products;

public sealed record GetProductsQuery(
    int Page,
    int PageSize,
    Guid? CategoryId,
    Guid? OccasionId,
    Guid? StoreId,
    bool? InStock) : IRequest<OperationResult<PagedResponse<ProductListItemResponse>>>;
