using Carter;
using Catalog_Service.Contracts.Products;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Catalog_Service.Features.Products;

public sealed class ProductsEndpoint : ICarterModule
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
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductsQuery(page ?? 1, pageSize ?? 20, categoryId, occasionId, storeId, inStock), cancellationToken);

            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<OperationResult<PagedResponse<ProductListItemResponse>>>();
    }
}
