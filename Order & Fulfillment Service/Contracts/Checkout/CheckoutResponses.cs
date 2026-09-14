using System.Text.Json.Serialization;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Contracts.Checkout;

public sealed record CheckoutDeliveryAddressResponse(
    Guid? AddressId,
    bool IsGift,
    string RecipientName,
    string Phone,
    string AddressLine,
    string City,
    string Area);

/// <summary>Named like a cart line so the app can reuse its cart line model.</summary>
public sealed record CheckoutItemResponse(
    Guid ProductId,
    string Name,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineSubtotal);

public sealed record CheckoutSummaryResponse(
    CheckoutDeliveryAddressResponse DeliveryAddress,
    IReadOnlyList<CheckoutItemResponse> Items,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total);

public sealed record PlacedOrderResponse(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    PaymentMethodType PaymentMethod,
    PaymentStatus PaymentStatus,
    bool PaymentRequired,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total);

public sealed record UnavailableCheckoutItemResponse(
    Guid ProductId,
    string Name,
    int RequestedQuantity,
    int AvailableQuantity);

/// <summary>The <c>data</c> of every checkout 409 and 503; the app switches on <see cref="Code"/>.</summary>
public sealed record CheckoutErrorResponse(
    string Code,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<UnavailableCheckoutItemResponse>? Items = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] CheckoutSummaryResponse? Summary = null);
