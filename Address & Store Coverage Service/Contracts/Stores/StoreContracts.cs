namespace Address___Store_Coverage_Service.Contracts.Stores;

public sealed record StoreCoverageAreaRequest(
    string City,
    string Area,
    decimal? MinLat,
    decimal? MaxLat,
    decimal? MinLng,
    decimal? MaxLng);

public sealed record StoreRequest(
    string Name,
    string Location,
    decimal? Lat,
    decimal? Lng,
    IReadOnlyList<StoreCoverageAreaRequest>? CoverageAreas);

public sealed record StoreCoverageAreaResponse(
    Guid Id,
    string City,
    string Area,
    decimal? MinLat,
    decimal? MaxLat,
    decimal? MinLng,
    decimal? MaxLng,
    bool IsActive);

public sealed record StoreResponse(
    Guid Id,
    string Name,
    string Location,
    decimal? Lat,
    decimal? Lng,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<StoreCoverageAreaResponse> CoverageAreas);

public sealed record CoverageDiagnosticsResponse(
    IReadOnlyList<CoverageGapResponse> Gaps,
    IReadOnlyList<CoverageOverlapResponse> Overlaps);

public sealed record CoverageGapResponse(string City, string Area);

public sealed record CoverageOverlapResponse(string City, string Area, IReadOnlyList<Guid> StoreIds);
