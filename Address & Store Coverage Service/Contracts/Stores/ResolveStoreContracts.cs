namespace Address___Store_Coverage_Service.Contracts.Stores;

public sealed record ResolveStoreRequest(
    string? City,
    string? Area,
    decimal? Lat,
    decimal? Lng);

public sealed record ResolveStoreResponse(
    Guid? StoreId,
    bool IsServiceable,
    string ResolutionType,
    IReadOnlyList<Guid> MatchingStoreIds);
