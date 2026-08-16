using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Occasions;

public sealed class GetOccasionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/occasions", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOccasionsQuery(), cancellationToken);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("GetOccasions")
        .WithTags("Occasions")
        .WithSummary("List active occasions")
        .Produces<OperationResult<IReadOnlyList<OccasionResponse>>>();
    }
}
