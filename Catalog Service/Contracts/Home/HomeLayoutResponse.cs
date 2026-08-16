namespace Catalog_Service.Contracts.Home;

public sealed record HomeSectionResponse(
    string Type,
    Guid Id,
    string? Title,
    int Order,
    bool Enabled,
    object? Payload);
