namespace Catalog_Service.Contracts.Home;

public sealed record HomeSectionResponse(
    Guid Id,
    string Type,
    string? Title,
    int Order,
    bool Enabled,
    Guid? OccasionId,
    Guid? CategoryId);


