using Carter;
using Catalog_Service.Contracts.Products;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Products.List;

public sealed class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
            int? page,
            int? pageSize,
            Guid? categoryId,
            Guid? occasionId,
            Guid? storeId,
            bool? inStock,
            string? search,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetProductsQuery(page ?? 1, pageSize ?? 20, categoryId, occasionId, storeId, inStock, search),
                cancellationToken);

            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<OperationResult<PagedResponse<ProductListItemResponse>>>();
    }
}
