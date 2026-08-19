namespace Address___Store_Coverage_Service.Contracts.Addresses;

public sealed record CreateAddressRequest(
    string RecipientName,
    string Phone,
    string AddressLine,
    string City,
    string Area,
    decimal? Lat,
    decimal? Lng,
    string? Label);
