using System.Net;
using System.Text.Json;

namespace Order___Fulfillment_Service.Infrastructure.Clients;

public enum DownstreamStatus
{
    Found,
    NotFound,
    Unavailable
}

public sealed record DownstreamResult<T>(DownstreamStatus Status, T? Value)
{
    public static DownstreamResult<T> Found(T value) => new(DownstreamStatus.Found, value);
    public static readonly DownstreamResult<T> NotFound = new(DownstreamStatus.NotFound, default);
    public static readonly DownstreamResult<T> Unavailable = new(DownstreamStatus.Unavailable, default);
}

internal static class DownstreamHttp
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Sends the request and unwraps the OperationResult "data" payload. A 404 is NotFound; any other
    /// failure, a timeout or an unreadable body is Unavailable so checkout answers 503 instead of guessing.
    /// </summary>
    public static async Task<DownstreamResult<T>> SendAsync<T>(
        HttpClient httpClient,
        HttpRequestMessage request,
        ILogger logger,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return DownstreamResult<T>.NotFound;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "{Method} {Uri} returned {StatusCode}.",
                    request.Method,
                    request.RequestUri,
                    (int)response.StatusCode);
                return DownstreamResult<T>.Unavailable;
            }

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>(SerializerOptions, cancellationToken);
            return envelope?.Data is null
                ? DownstreamResult<T>.Unavailable
                : DownstreamResult<T>.Found(envelope.Data);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or JsonException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogWarning(exception, "{Method} {Uri} is unreachable.", request.Method, request.RequestUri);
            return DownstreamResult<T>.Unavailable;
        }
    }

    private sealed record Envelope<TData>(TData? Data) where TData : class;
}
