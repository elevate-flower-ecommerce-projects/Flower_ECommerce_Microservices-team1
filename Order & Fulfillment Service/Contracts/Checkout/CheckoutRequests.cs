using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Contracts.Checkout;

/// <summary>Same fields and rules as a saved address, plus the optional gift message.</summary>
public sealed record GiftDetailsRequest(
    string? RecipientName,
    string? Phone,
    string? AddressLine,
    string? City,
    string? Area,
    decimal? Lat,
    decimal? Lng,
    string? Message);

/// <summary>Nothing for the default address, <c>AddressId</c> for another saved one, or <c>Gift</c>.</summary>
public sealed record CheckoutPreviewRequest(Guid? AddressId, GiftDetailsRequest? Gift);

public sealed record PlaceOrderRequest(
    Guid? AddressId,
    GiftDetailsRequest? Gift,
    PaymentMethodType? PaymentMethod,
    decimal? ExpectedTotal);
