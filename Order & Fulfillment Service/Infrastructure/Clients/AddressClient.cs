using System.Net.Http.Json;

namespace Order___Fulfillment_Service.Infrastructure.Clients;

public sealed record AddressSnapshot(
    Guid Id,
    string RecipientName,
    string Phone,
    string AddressLine,
    string City,
    string Area,
    decimal? Lat,
    decimal? Lng);

public sealed record StoreResolution(Guid? StoreId, bool IsServiceable);

public interface IAddressClient
{
    /// <summary>NotFound when the address does not exist or belongs to someone else.</summary>
    Task<DownstreamResult<AddressSnapshot>> GetAddressAsync(Guid addressId, CancellationToken cancellationToken);

    /// <summary>The default address, or the most recently used one; NotFound when none is saved.</summary>
    Task<DownstreamResult<Guid>> GetPreferredAddressIdAsync(CancellationToken cancellationToken);

    Task<DownstreamResult<StoreResolution>> ResolveStoreAsync(
        string city,
        string area,
        decimal? lat,
        decimal? lng,
        CancellationToken cancellationToken);
}

public sealed class AddressClient(HttpClient httpClient, ILogger<AddressClient> logger) : IAddressClient
{
    public Task<DownstreamResult<AddressSnapshot>> GetAddressAsync(Guid addressId, CancellationToken cancellationToken)
        => DownstreamHttp.SendAsync<AddressSnapshot>(
            httpClient,
            new HttpRequestMessage(HttpMethod.Get, $"users/me/addresses/{addressId}"),
            logger,
            cancellationToken);

    public async Task<DownstreamResult<Guid>> GetPreferredAddressIdAsync(CancellationToken cancellationToken)
    {
        var list = await DownstreamHttp.SendAsync<List<AddressListItem>>(
            httpClient,
            new HttpRequestMessage(HttpMethod.Get, "users/me/addresses"),
            logger,
            cancellationToken);

        if (list.Status is not DownstreamStatus.Found)
            return list.Status is DownstreamStatus.NotFound ? DownstreamResult<Guid>.NotFound : DownstreamResult<Guid>.Unavailable;

        // The Address service already orders the list default first, then most recently used.
        var preferred = list.Value!.OrderByDescending(address => address.IsDefault).FirstOrDefault();
        return preferred is null
            ? DownstreamResult<Guid>.NotFound
            : DownstreamResult<Guid>.Found(preferred.Id);
    }

    public Task<DownstreamResult<StoreResolution>> ResolveStoreAsync(
        string city,
        string area,
        decimal? lat,
        decimal? lng,
        CancellationToken cancellationToken)
        => DownstreamHttp.SendAsync<StoreResolution>(
            httpClient,
            new HttpRequestMessage(HttpMethod.Post, "stores/resolve")
            {
                Content = JsonContent.Create(new { city, area, lat, lng })
            },
            logger,
            cancellationToken);

    private sealed record AddressListItem(Guid Id, bool IsDefault);
}
