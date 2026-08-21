using Address___Store_Coverage_Service.Contracts.Addresses;
using Address___Store_Coverage_Service.Entities;

namespace Address___Store_Coverage_Service.Features.Addresses;

public static class AddressMapping
{
    public static AddressResponse ToResponse(UserAddress address) => new(
        address.Id,
        address.RecipientName,
        address.Phone,
        address.AddressLine,
        address.City,
        address.Area,
        address.Lat,
        address.Lng,
        address.Label,
        address.ServingStoreId,
        address.IsServiceable,
        address.IsDefault,
        address.CreatedAtUtc,
        address.LastUsedAtUtc);

    public static AddressListItemResponse ToListItem(UserAddress address) => new(
        address.Id,
        BuildShortLabel(address),
        address.RecipientName,
        address.City,
        address.Area,
        address.Label,
        address.IsDefault,
        address.IsServiceable,
        address.ServingStoreId,
        address.CreatedAtUtc,
        address.LastUsedAtUtc);

    private static string BuildShortLabel(UserAddress address)
        => $"{address.RecipientName} - {address.City}/{address.Area}";
}
