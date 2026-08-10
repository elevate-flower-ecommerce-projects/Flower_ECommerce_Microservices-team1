namespace Catalog_Service.Contracts.Home;

public sealed record HomeSectionResponse(
    string Type,
    Guid Id,
    string? Title,
    int Order,
    bool Enabled,
    object? Payload);

public sealed record CategorySummary(Guid Id, string Name, string? ImageUrl, string DeepLink);

public sealed record ProductSummary(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    Guid? CategoryId,
    Guid? OccasionId,
    string DeepLink);

public sealed record OccasionSummary(Guid Id, string Name, string? ImageUrl, string DeepLink);

public sealed record BannerPayload(string ImageUrl, string DeepLink);

public sealed record RailPayload<T>(IReadOnlyList<T> Items, ViewAllAction ViewAll);

public sealed record ViewAllAction(string Label, string DeepLink);
