namespace Catalog_Service.Features.Occasions;

public sealed record OccasionResponse(
    Guid Id,
    string Name,
    string ImageUrl,
    int DisplayOrder);
