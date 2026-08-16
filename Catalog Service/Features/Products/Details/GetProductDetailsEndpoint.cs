using Carter;
using Catalog_Service.Contracts.Products;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Products.Details;

public sealed class GetProductDetailsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (
            Guid id,
            Guid? storeId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductDetailsQuery(id, storeId), cancellationToken);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("GetProductDetails")
        .WithTags("Products")
        .Produces<OperationResult<ProductDetailResponse>>()
        .Produces<OperationResult>(StatusCodes.Status404NotFound);
    }
}
