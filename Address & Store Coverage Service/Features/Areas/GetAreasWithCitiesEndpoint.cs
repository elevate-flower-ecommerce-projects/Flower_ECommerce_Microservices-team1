using Address___Store_Coverage_Service.Features.Areas.DTOs;
using Address___Store_Coverage_Service.Features.Areas.Queries;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Areas;

public sealed class GetAreasWithCitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/areas", HandleAsync)
            .AllowAnonymous()
            .WithName("GetAreasWithCitiesApi")
            .WithTags("Lookups")
            .Produces<OperationResult<IReadOnlyList<AreaWithCitiesDto>>>();

        app.MapGet("/areas", HandleAsync)
            .AllowAnonymous()
            .WithName("GetAreasWithCities")
            .WithTags("Lookups")
            .Produces<OperationResult<IReadOnlyList<AreaWithCitiesDto>>>();

        app.MapGet("/address/api/areas", HandleAsync)
            .AllowAnonymous()
            .WithName("GetAreasWithCitiesAddressApi")
            .WithTags("Lookups")
            .Produces<OperationResult<IReadOnlyList<AreaWithCitiesDto>>>();
    }

    private static async Task<IResult> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAreasWithCitiesQuery(), cancellationToken);
        return result.ToHttpResult();
    }
}
