using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Contracts.Drivers;

public sealed record DriverOrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    string CustomerArea,
    int ItemCount,
    OrderStatus Status);

public sealed record LocationResponse(decimal Latitude, decimal Longitude);

public sealed record DriverOrderItemResponse(string ProductName, int Quantity, decimal UnitPrice);

public sealed record DriverOrderDetailsResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    IReadOnlyList<DriverOrderItemResponse> Items,
    string RecipientName,
    string RecipientPhone,
    string StoreName,
    string StoreAddress,
    LocationResponse PickupLocation,
    string DeliveryAddressLine,
    string DeliveryCity,
    string DeliveryArea,
    LocationResponse DropOffLocation);
