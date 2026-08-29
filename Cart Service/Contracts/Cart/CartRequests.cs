namespace Cart_Service.Contracts.Cart;

public sealed record AddCartItemRequest(Guid ProductId, int Quantity, Guid? StoreId);

public sealed record UpdateCartItemQuantityRequest(int Quantity, Guid? StoreId);

/// <summary>Returned with a 409 so the client can correct its stepper to the real ceiling.</summary>
public sealed record StockLimitResponse(Guid ProductId, int RequestedQuantity, int AvailableQuantity);
