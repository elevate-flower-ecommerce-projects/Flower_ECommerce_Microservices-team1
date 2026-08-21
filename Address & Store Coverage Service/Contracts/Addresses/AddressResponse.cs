namespace Address___Store_Coverage_Service.Contracts.Addresses;

public sealed record AddressResponse(
    Guid Id,
    string RecipientName,
    string Phone,
    string AddressLine,
    string City,
    string Area,
    decimal? Lat,
    decimal? Lng,
    string? Label,
    Guid? ServingStoreId,
    bool IsServiceable,
    bool IsDefault,
    DateTime CreatedAtUtc,
    DateTime? LastUsedAtUtc);

public sealed record AddressListItemResponse(
    Guid Id,
    string ShortLabel,
    string RecipientName,
    string City,
    string Area,
    string? Label,
    bool IsDefault,
    bool IsServiceable,
    Guid? ServingStoreId,
    DateTime CreatedAtUtc,
    DateTime? LastUsedAtUtc);
