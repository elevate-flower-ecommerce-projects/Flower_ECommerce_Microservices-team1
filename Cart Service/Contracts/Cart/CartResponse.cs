namespace Cart_Service.Contracts.Cart;

public sealed record CartResponse(
    IReadOnlyList<CartLineResponse> Lines,
    decimal Subtotal,
    decimal? DeliveryFee,
    decimal Total,
    bool IsEmpty,
    bool HasChanges,
    bool PricingUnavailable);

public sealed record CartLineResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineSubtotal,
    bool InStock,
    int AvailableQuantity,
    bool PriceChanged,
    bool OutOfStock);
