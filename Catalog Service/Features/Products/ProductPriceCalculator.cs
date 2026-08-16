namespace Catalog_Service.Features.Products;

public sealed record ProductPrice(
    decimal Price,
    decimal? DiscountedPrice,
    decimal? DiscountPercent);

public static class ProductPriceCalculator
{
    public static ProductPrice Calculate(
        decimal price,
        decimal? discountPercent,
        DateTime? discountStartsAtUtc,
        DateTime? discountEndsAtUtc,
        DateTime utcNow)
    {
        var isActive = discountPercent is > 0 and <= 100
            && (discountStartsAtUtc is null || discountStartsAtUtc <= utcNow)
            && (discountEndsAtUtc is null || discountEndsAtUtc >= utcNow);

        if (!isActive)
            return new ProductPrice(price, null, null);

        return new ProductPrice(
            price,
            Math.Round(price * (1 - discountPercent!.Value / 100), 2, MidpointRounding.AwayFromZero),
            discountPercent);
    }
}
