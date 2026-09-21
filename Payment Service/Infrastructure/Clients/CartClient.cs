namespace Payment_Service.Infrastructure.Clients;

public interface ICartClient
{
    /// <summary>Empties a customer's cart after their card order is paid. Safe to call again.</summary>
    Task<bool> ClearCartAsync(string userId, CancellationToken cancellationToken);
}

public sealed class CartClient(HttpClient httpClient, ILogger<CartClient> logger) : ICartClient
{
    public async Task<bool> ClearCartAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var path = $"internal/carts/{Uri.EscapeDataString(userId)}/clear";
            using var response = await httpClient.PostAsync(path, content: null, cancellationToken);
            if (response.IsSuccessStatusCode)
                return true;

            logger.LogWarning("Clearing the cart returned {StatusCode}.", (int)response.StatusCode);
            return false;
        }
        catch (Exception exception) when (
            exception is HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // The order is already paid and recorded; an uncleared cart is a nuisance, not a failure.
            logger.LogWarning(exception, "The cart service is unreachable while clearing the cart.");
            return false;
        }
    }
}
