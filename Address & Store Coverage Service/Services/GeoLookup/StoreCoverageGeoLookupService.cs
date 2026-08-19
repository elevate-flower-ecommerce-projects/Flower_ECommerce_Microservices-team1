using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Services.GeoLookup;

public sealed class StoreCoverageGeoLookupService(IUnitOfWork<AddressDbContext> unitOfWork) : IGeoLookupService
{
    public async Task<GeoLookupResult> ResolveAsync(GeoLookupRequest request, CancellationToken cancellationToken)
    {
        var city = request.City.Trim().ToUpper();
        var area = request.Area.Trim().ToUpper();

        var query = unitOfWork.Repository<StoreCoverageArea, Guid>()
            .Query()
            .Where(coverage => coverage.IsActive
                && coverage.City.ToUpper() == city
                && coverage.Area.ToUpper() == area);

        if (request.Lat is not null && request.Lng is not null)
        {
            var lat = request.Lat.Value;
            var lng = request.Lng.Value;
            query = query.Where(coverage =>
                coverage.MinLat == null || coverage.MaxLat == null || coverage.MinLng == null || coverage.MaxLng == null
                || (lat >= coverage.MinLat && lat <= coverage.MaxLat && lng >= coverage.MinLng && lng <= coverage.MaxLng));
        }

        var storeId = await query
            .OrderBy(coverage => coverage.Area)
            .Select(coverage => (Guid?)coverage.StoreId)
            .FirstOrDefaultAsync(cancellationToken);

        return new GeoLookupResult(storeId);
    }
}
