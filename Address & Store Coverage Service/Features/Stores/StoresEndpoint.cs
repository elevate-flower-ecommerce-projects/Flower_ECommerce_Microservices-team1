using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Features.Stores.Admin.Create;
using Address___Store_Coverage_Service.Features.Stores.Admin.Deactivate;
using Address___Store_Coverage_Service.Features.Stores.Admin.Diagnostics;
using Address___Store_Coverage_Service.Features.Stores.Admin.Get;
using Address___Store_Coverage_Service.Features.Stores.Admin.List;
using Address___Store_Coverage_Service.Features.Stores.Admin.Update;
using Address___Store_Coverage_Service.Features.Stores.Resolve;
using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Address___Store_Coverage_Service.Features.Stores;

public sealed class StoresEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/admin/stores")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .WithTags("Admin Stores");

        admin.MapGet("", async (bool? includeInactive, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ListStoresQuery(includeInactive ?? false), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ListAdminStores")
        .Produces<OperationResult<IReadOnlyList<StoreResponse>>>();

        admin.MapGet("/coverage-diagnostics", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCoverageDiagnosticsQuery(), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("GetCoverageDiagnostics")
        .Produces<OperationResult<CoverageDiagnosticsResponse>>();

        admin.MapGet("/{storeId:guid}", async (Guid storeId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetStoreQuery(storeId), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("GetAdminStore")
        .Produces<OperationResult<StoreResponse>>()
        .Produces<OperationResult>(StatusCodes.Status404NotFound);

        admin.MapPost("", async (StoreRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CreateStoreCommand(
                request.Name,
                request.Location,
                request.Lat,
                request.Lng,
                ToCreateCoverage(request.CoverageAreas)), cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("CreateAdminStore")
        .Produces<OperationResult<StoreResponse>>(StatusCodes.Status201Created)
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
        .Produces<OperationResult>(StatusCodes.Status409Conflict);

        admin.MapPut("/{storeId:guid}", async (Guid storeId, StoreRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateStoreCommand(
                storeId,
                request.Name,
                request.Location,
                request.Lat,
                request.Lng,
                ToUpdateCoverage(request.CoverageAreas)), cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("UpdateAdminStore")
        .Produces<OperationResult<StoreResponse>>()
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status409Conflict);

        admin.MapDelete("/{storeId:guid}", async (Guid storeId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeactivateStoreCommand(storeId), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("DeactivateAdminStore")
        .Produces<OperationResult>()
        .Produces<OperationResult>(StatusCodes.Status404NotFound)
        .Produces<OperationResult>(StatusCodes.Status409Conflict);

        app.MapPost("/stores/resolve", async (ResolveStoreRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ResolveStoreQuery(request.City, request.Area, request.Lat, request.Lng), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer,Admin" })
        .WithName("ResolveStore")
        .WithTags("Address & Coverage")
        .Produces<OperationResult<ResolveStoreResponse>>()
        .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity);
    }

    private static IReadOnlyList<CreateStoreCoverageArea> ToCreateCoverage(IReadOnlyList<StoreCoverageAreaRequest>? coverageAreas)
        => (coverageAreas ?? [])
            .Select(area => new CreateStoreCoverageArea(area.City, area.Area, area.MinLat, area.MaxLat, area.MinLng, area.MaxLng))
            .ToList();

    private static IReadOnlyList<UpdateStoreCoverageArea> ToUpdateCoverage(IReadOnlyList<StoreCoverageAreaRequest>? coverageAreas)
        => (coverageAreas ?? [])
            .Select(area => new UpdateStoreCoverageArea(area.City, area.Area, area.MinLat, area.MaxLat, area.MinLng, area.MaxLng))
            .ToList();
}
