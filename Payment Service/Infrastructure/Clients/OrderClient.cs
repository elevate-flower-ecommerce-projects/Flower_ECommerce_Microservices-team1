using System.Net;
using System.Net.Http.Json;
using Payment_Service.Contracts.Internal;

namespace Payment_Service.Infrastructure.Clients;

/// <summary>
/// The result of telling the order service that a payment succeeded.
/// Refused means the order service looked at the order and said no; that is an answer, not an
/// outage, so it must not be retried. Unavailable is worth retrying.
/// </summary>
public enum MarkOrderPaidOutcome
{
    Confirmed,
    NotFound,
    Refused,
    Unavailable
}

public interface IOrderClient
{
    Task<DownstreamResult<InternalOrderSnapshot>> GetOrderAsync(Guid orderId, CancellationToken cancellationToken);

    Task<MarkOrderPaidOutcome> MarkOrderPaidAsync(
        Guid orderId,
        decimal amount,
        string provider,
        string reference,
        CancellationToken cancellationToken);
}

public sealed class OrderClient(HttpClient httpClient, ILogger<OrderClient> logger) : IOrderClient
{
    public Task<DownstreamResult<InternalOrderSnapshot>> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
        => DownstreamHttp.SendAsync<InternalOrderSnapshot>(
            httpClient,
            new HttpRequestMessage(HttpMethod.Get, $"internal/orders/{orderId}"),
            logger,
            cancellationToken);

    public async Task<MarkOrderPaidOutcome> MarkOrderPaidAsync(
        Guid orderId,
        decimal amount,
        string provider,
        string reference,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = new MarkOrderPaidRequest(provider, reference, amount);
            using var response = await httpClient.PostAsJsonAsync(
                $"internal/orders/{orderId}/payment-succeeded",
                payload,
                DownstreamHttp.SerializerOptions,
                cancellationToken);

            if (response.IsSuccessStatusCode)
                return MarkOrderPaidOutcome.Confirmed;

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    logger.LogError("The order service does not know order {OrderId}.", orderId);
                    return MarkOrderPaidOutcome.NotFound;

                case HttpStatusCode.Conflict:
                    logger.LogError("The order service refused to mark order {OrderId} as paid.", orderId);
                    return MarkOrderPaidOutcome.Refused;

                default:
                    logger.LogWarning(
                        "Marking order {OrderId} as paid returned {StatusCode}.",
                        orderId,
                        (int)response.StatusCode);
                    return MarkOrderPaidOutcome.Unavailable;
            }
        }
        catch (Exception exception) when (
            exception is HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogWarning(exception, "The order service is unreachable while confirming order {OrderId}.", orderId);
            return MarkOrderPaidOutcome.Unavailable;
        }
    }
}
