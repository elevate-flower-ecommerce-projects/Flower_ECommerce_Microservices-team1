namespace Cart_Service.Features.Cart;

public static class CartInputValidator
{
    public const int MaxQuantityPerLine = 99;

    /// <summary>Validates a write request. Returns an empty dictionary when the input is valid.</summary>
    public static Dictionary<string, string[]> Validate(Guid? productId, int quantity, Guid? storeId, bool allowZero)
    {
        var errors = new Dictionary<string, string[]>();

        if (productId is not null && productId == Guid.Empty)
        {
            errors[nameof(productId)] = ["Product id is required."];
        }

        var minimum = allowZero ? 0 : 1;
        if (quantity < minimum)
        {
            errors[nameof(quantity)] = [$"Quantity must be {minimum} or greater."];
        }
        else if (quantity > MaxQuantityPerLine)
        {
            errors[nameof(quantity)] = [$"Quantity must not exceed {MaxQuantityPerLine}."];
        }

        // Stock cannot be validated without knowing which store fulfils the order.
        if (storeId is null || storeId == Guid.Empty)
        {
            errors[nameof(storeId)] = ["Store id is required."];
        }

        return errors;
    }
}
