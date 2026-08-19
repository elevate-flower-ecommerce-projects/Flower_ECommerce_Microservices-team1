namespace Address___Store_Coverage_Service.Services.GeoLookup;

public sealed record GeoLookupRequest(
    string City,
    string Area,
    decimal? Lat,
    decimal? Lng);

public sealed record GeoLookupResult(Guid? ServingStoreId)
{
    public bool IsServiceable => ServingStoreId is not null;
}

public interface IGeoLookupService
{
    Task<GeoLookupResult> ResolveAsync(GeoLookupRequest request, CancellationToken cancellationToken);
}
