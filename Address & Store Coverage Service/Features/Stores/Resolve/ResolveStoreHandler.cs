using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Features.Stores.Admin;
using Address___Store_Coverage_Service.Services.GeoLookup;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Stores.Resolve;

public sealed class ResolveStoreHandler(IGeoLookupService geoLookupService)
    : IRequestHandler<ResolveStoreQuery, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(ResolveStoreQuery request, CancellationToken cancellationToken)
    {
        var resolveRequest = new ResolveStoreRequest(request.City, request.Area, request.Lat, request.Lng);
        var errors = StoreRequestValidator.ValidateResolve(resolveRequest);
        if (errors.Count > 0)
            return OperationResultFactory.Validation<object>(errors, "Store resolution validation failed.", "Store resolution validation failed.");

        var result = await geoLookupService.ResolveAsync(
            new GeoLookupRequest(request.City, request.Area, request.Lat, request.Lng),
            cancellationToken);

        return OperationResultFactory.Success<object>(
            new ResolveStoreResponse(
                result.ServingStoreId,
                result.IsServiceable,
                result.ResolutionType,
                result.MatchingStoreIds));
    }
}
