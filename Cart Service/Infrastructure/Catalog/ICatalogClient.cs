namespace Cart_Service.Infrastructure.Catalog;

public interface ICatalogClient
{
    /// <summary>
    /// Reads a product from the Catalog service.
    /// Returns <see cref="CatalogLookupStatus.NotFound"/> when the product does not exist or is
    /// inactive, and <see cref="CatalogLookupStatus.Unavailable"/> when the Catalog service could
    /// not be reached — callers must not turn the latter into a 500.
    /// </summary>
    Task<CatalogLookupResult> GetProductAsync(Guid productId, Guid? storeId, CancellationToken cancellationToken);
}

public enum CatalogLookupStatus
{
    Found,
    NotFound,
    Unavailable
}

public sealed record CatalogLookupResult(CatalogLookupStatus Status, CatalogProductDto? Product)
{
    public static CatalogLookupResult Found(CatalogProductDto product) => new(CatalogLookupStatus.Found, product);
    public static readonly CatalogLookupResult NotFound = new(CatalogLookupStatus.NotFound, null);
    public static readonly CatalogLookupResult Unavailable = new(CatalogLookupStatus.Unavailable, null);
}
