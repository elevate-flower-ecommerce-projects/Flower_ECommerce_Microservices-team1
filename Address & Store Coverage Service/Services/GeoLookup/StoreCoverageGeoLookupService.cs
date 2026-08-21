using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Services.GeoLookup;

public sealed class StoreCoverageGeoLookupService(IUnitOfWork<AddressDbContext> unitOfWork) : IGeoLookupService
{
    public async Task<GeoLookupResult> ResolveAsync(GeoLookupRequest request, CancellationToken cancellationToken)
    {
        var matches = await ResolveCoverageMatchesAsync(request, cancellationToken);
        if (matches.Count > 0)
        {
            var storeIds = matches.Select(coverage => coverage.StoreId).Distinct().ToList();
            return new GeoLookupResult(storeIds[0], "CoverageArea", storeIds);
        }

        if (request.Lat is not null && request.Lng is not null)
        {
            var nearestStore = await ResolveNearestStoreAsync(request.Lat.Value, request.Lng.Value, cancellationToken);
            if (nearestStore is not null)
            {
                return new GeoLookupResult(nearestStore.Value, "NearestStore", [nearestStore.Value]);
            }
        }

        return new GeoLookupResult(null, "Unresolved", []);
    }

    private async Task<List<StoreCoverageArea>> ResolveCoverageMatchesAsync(
        GeoLookupRequest request,
        CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<StoreCoverageArea, Guid>()
            .Query()
            .Include(coverage => coverage.Store)
            .Where(coverage => coverage.IsActive && coverage.Store != null && coverage.Store.IsActive);

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim().ToUpper();
            query = query.Where(coverage => coverage.City.ToUpper() == city);
        }

        if (!string.IsNullOrWhiteSpace(request.Area))
        {
            var area = request.Area.Trim().ToUpper();
            query = query.Where(coverage => coverage.Area.ToUpper() == area);
        }

        if (request.Lat is not null && request.Lng is not null)
        {
            var lat = request.Lat.Value;
            var lng = request.Lng.Value;
            query = query.Where(coverage =>
                coverage.MinLat == null || coverage.MaxLat == null || coverage.MinLng == null || coverage.MaxLng == null
                || (lat >= coverage.MinLat && lat <= coverage.MaxLat && lng >= coverage.MinLng && lng <= coverage.MaxLng));
        }

        return await query
            .OrderBy(coverage => coverage.City)
            .ThenBy(coverage => coverage.Area)
            .ThenBy(coverage => coverage.StoreId)
            .ToListAsync(cancellationToken);
    }

    private async Task<Guid?> ResolveNearestStoreAsync(
        decimal lat,
        decimal lng,
        CancellationToken cancellationToken)
    {
        var stores = await unitOfWork.Repository<Store, Guid>()
            .Query()
            .Where(store => store.IsActive && store.Lat != null && store.Lng != null)
            .Select(store => new StoreLocation(store.Id, store.Lat!.Value, store.Lng!.Value))
            .ToListAsync(cancellationToken);

        if (stores.Count == 0)
            return null;

        return stores
            .OrderBy(store => DistanceSquared(lat, lng, store.Lat, store.Lng))
            .ThenBy(store => store.Id)
            .Select(store => (Guid?)store.Id)
            .FirstOrDefault();
    }

    private static decimal DistanceSquared(decimal lat, decimal lng, decimal storeLat, decimal storeLng)
    {
        var latDelta = lat - storeLat;
        var lngDelta = lng - storeLng;
        return (latDelta * latDelta) + (lngDelta * lngDelta);
    }

    private sealed record StoreLocation(Guid Id, decimal Lat, decimal Lng);
}
