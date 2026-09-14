namespace Order___Fulfillment_Service.Infrastructure.Clients;

public sealed record CartSnapshot(IReadOnlyList<CartSnapshotLine> Lines, bool IsEmpty, bool PricingUnavailable);

public sealed record CartSnapshotLine(
    Guid ProductId,
    string Name,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    bool InStock,
    int AvailableQuantity,
    bool OutOfStock);

public interface ICartClient
{
    /// <summary>The caller's cart priced and stock-checked at <paramref name="storeId"/>.</summary>
    Task<DownstreamResult<CartSnapshot>> GetCartAsync(Guid storeId, CancellationToken cancellationToken);

    Task<bool> ClearCartAsync(CancellationToken cancellationToken);
}

public sealed class CartClient(HttpClient httpClient, ILogger<CartClient> logger) : ICartClient
{
    public Task<DownstreamResult<CartSnapshot>> GetCartAsync(Guid storeId, CancellationToken cancellationToken)
        => DownstreamHttp.SendAsync<CartSnapshot>(
            httpClient,
            new HttpRequestMessage(HttpMethod.Get, $"?storeId={storeId}"),
            logger,
            cancellationToken);

    public async Task<bool> ClearCartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.DeleteAsync(string.Empty, cancellationToken);
            if (response.IsSuccessStatusCode)
                return true;

            logger.LogWarning("Clearing the cart returned {StatusCode}.", (int)response.StatusCode);
            return false;
        }
        catch (Exception exception) when (
            exception is HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogWarning(exception, "Cart service is unreachable while clearing the cart.");
            return false;
        }
    }
}
