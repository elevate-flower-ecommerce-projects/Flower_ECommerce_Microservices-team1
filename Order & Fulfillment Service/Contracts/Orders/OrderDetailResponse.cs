namespace Order___Fulfillment_Service.Contracts.Orders;

public sealed record OrderDetailResponse(
    Guid Id,
    string OrderNumber,
    DateTime PlacedAtUtc,
    OrderStatusResponse Status,
    IReadOnlyList<OrderLineItemResponse> Items,
    DeliveryAddressResponse DeliveryAddress,
    GiftRecipientResponse? GiftRecipient,
    PaymentMethodResponse PaymentMethod,
    PriceBreakdownResponse PriceBreakdown,
    bool CanTrack,
    string? TrackingUrl);

public sealed record OrderLineItemResponse(
    Guid ProductId,
    string ProductName,
    string? ThumbnailUrl,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record DeliveryAddressResponse(
    string RecipientName,
    string Phone,
    string AddressLine,
    string City,
    string Area);

public sealed record GiftRecipientResponse(
    string Name,
    string Phone,
    string? Message);

public sealed record PaymentMethodResponse(
    string Code,
    string Label,
    string? Last4);

public sealed record PriceBreakdownResponse(
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total);

public sealed record OrderStatusResponse(
    string Code,
    string Label,
    string Color);