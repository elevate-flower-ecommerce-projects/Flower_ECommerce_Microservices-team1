using System.Net;
using System.Text.Json;
using Flower.Common.StandardizedResponse;

namespace Cart_Service.Infrastructure.Catalog;

public sealed class CatalogClient(HttpClient httpClient, ILogger<CatalogClient> logger) : ICatalogClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<CatalogLookupResult> GetProductAsync(
        Guid productId,
        Guid? storeId,
        CancellationToken cancellationToken)
    {
        var requestUri = storeId is null
            ? $"products/{productId}"
            : $"products/{productId}?storeId={storeId}";

        try
        {
            using var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound)
            {
                return CatalogLookupResult.NotFound;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Catalog returned {StatusCode} for product {ProductId}.",
                    (int)response.StatusCode,
                    productId);
                return CatalogLookupResult.Unavailable;
            }

            // The Catalog service wraps every payload in OperationResult<T>; the product sits in "data".
            var envelope = await response.Content.ReadFromJsonAsync<OperationResult<CatalogProductDto>>(
                SerializerOptions,
                cancellationToken);

            return envelope?.Data is null
                ? CatalogLookupResult.NotFound
                : CatalogLookupResult.Found(envelope.Data);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or JsonException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // A Catalog outage must degrade the cart, never break it.
            logger.LogWarning(exception, "Catalog service is unreachable while reading product {ProductId}.", productId);
            return CatalogLookupResult.Unavailable;
        }
    }
}
