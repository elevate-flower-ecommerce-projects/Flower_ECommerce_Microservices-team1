namespace Cart_Service.Features.Cart;

public static class CartMessages
{
    public const string CartLoaded = "Cart loaded successfully.";
    public const string CartEmpty = "Your cart is empty.";
    public const string ItemAdded = "Item added to your cart.";
    public const string QuantityUpdated = "Cart item quantity updated.";
    public const string ItemRemoved = "Item removed from your cart.";

    public const string ProductNotFound = "Product was not found.";
    public const string ItemNotFound = "Cart item was not found.";
    public const string OutOfStock = "This product is currently out of stock.";
    public const string QuantityExceedsStock = "Requested quantity exceeds the available stock.";
    public const string CatalogUnavailable = "Product information is temporarily unavailable. Please try again.";

    public const string MissingIdentity = "Missing user identity.";
    public const string ValidationFailed = "Cart request validation failed.";
}

public static class CartRoutes
{
    /// <summary>
    /// The gateway strips the "/cart" prefix (PathRemovePrefix), so routes are declared without it.
    /// Externally these are reachable as /api/v1/cart/...
    /// </summary>
    public const string Cart = "/";
    public const string Items = "/items";
    public const string Item = "/items/{itemId:guid}";
    public const string Tag = "Cart";
}
