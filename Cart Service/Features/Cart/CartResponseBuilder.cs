using Cart_Service.Contracts.Cart;
using Cart_Service.Infrastructure.Catalog;

namespace Cart_Service.Features.Cart;

public sealed class CartResponseBuilder(ICatalogClient catalogClient) : ICartResponseBuilder
{
    public static readonly CartResponse Empty = new([], 0m, null, 0m, true, false, false);

    public async Task<CartResponse> BuildAsync(
        Entities.Cart? cart,
        Guid? storeId,
        CancellationToken cancellationToken)
    {
        var items = cart?.Items.OrderBy(item => item.AddedAtUtc).ToList() ?? [];
        if (items.Count == 0)
        {
            return Empty;
        }

        // One Catalog call per line, issued in parallel. Carts are small (5-10 lines) so this is
        // cheaper than adding a batch endpoint to the Catalog service.
        var lookups = await Task.WhenAll(items.Select(item =>
            catalogClient.GetProductAsync(item.ProductId, storeId, cancellationToken)));

        var lines = new List<CartLineResponse>(items.Count);
        var pricingUnavailable = false;

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var lookup = lookups[index];

            var (unitPrice, inStock, availableQuantity) = lookup.Status switch
            {
                CatalogLookupStatus.Found =>
                    (lookup.Product!.EffectivePrice, lookup.Product.InStock, lookup.Product.AvailableQuantity),

                // The product was deleted or deactivated in the catalog: keep the line visible but
                // clearly unavailable so the customer can remove it.
                CatalogLookupStatus.NotFound =>
                    (item.UnitPriceSnapshot, false, 0),

                // Catalog is down: fall back to the snapshot and tell the client the numbers are stale.
                _ => (item.UnitPriceSnapshot, true, item.Quantity)
            };

            if (lookup.Status is CatalogLookupStatus.Unavailable)
            {
                pricingUnavailable = true;
            }

            var priceChanged = lookup.Status is CatalogLookupStatus.Found
                && unitPrice != item.UnitPriceSnapshot;

            lines.Add(new CartLineResponse(
                item.Id,
                item.ProductId,
                lookup.Product?.Name ?? item.ProductName,
                lookup.Product?.ImageUrls.FirstOrDefault() ?? item.ImageUrl,
                unitPrice,
                item.Quantity,
                unitPrice * item.Quantity,
                inStock,
                availableQuantity,
                priceChanged,
                OutOfStock: !inStock));
        }

        var subtotal = lines.Sum(line => line.LineSubtotal);

        // Delivery fee is owned by the Order & Fulfillment service and is out of scope here.
        decimal? deliveryFee = null;

        var hasChanges = lines.Any(line =>
            line.PriceChanged || line.OutOfStock || line.AvailableQuantity < line.Quantity);

        return new CartResponse(
            lines,
            subtotal,
            deliveryFee,
            subtotal + (deliveryFee ?? 0m),
            IsEmpty: false,
            hasChanges,
            pricingUnavailable);
    }
}
