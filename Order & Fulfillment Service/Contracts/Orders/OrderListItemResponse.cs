namespace Order___Fulfillment_Service.Contracts.Orders;

public sealed record OrderListItemResponse(
    Guid Id,
    string OrderNumber,
    DateTime PlacedAtUtc,
    int ItemCount,
    string? ThumbnailUrl,
    OrderStatusResponse Status,
    decimal Total);