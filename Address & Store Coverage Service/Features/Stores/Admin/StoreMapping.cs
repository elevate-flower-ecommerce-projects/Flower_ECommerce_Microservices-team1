using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Entities;

namespace Address___Store_Coverage_Service.Features.Stores.Admin;

public static class StoreMapping
{
    public static StoreResponse ToResponse(Store store) => new(
        store.Id,
        store.Name,
        store.Location,
        store.Lat,
        store.Lng,
        store.IsActive,
        store.CreatedAtUtc,
        store.UpdatedAtUtc,
        store.CoverageAreas
            .OrderBy(area => area.City)
            .ThenBy(area => area.Area)
            .Select(ToResponse)
            .ToList());

    public static StoreCoverageAreaResponse ToResponse(StoreCoverageArea coverage) => new(
        coverage.Id,
        coverage.City,
        coverage.Area,
        coverage.MinLat,
        coverage.MaxLat,
        coverage.MinLng,
        coverage.MaxLng,
        coverage.IsActive);
}
