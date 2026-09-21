using Microsoft.Extensions.Options;
using Payment_Service.Settings;

namespace Payment_Service.Infrastructure.Clients;

/// <summary>
/// Stamps the shared internal secret on every call to another Flower service. A provider webhook
/// carries no customer token, so this service has to identify itself instead of forwarding one.
/// </summary>
public sealed class InternalSecretHandler(IOptions<InternalApiOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!string.IsNullOrWhiteSpace(settings.Secret))
        {
            var headerName = string.IsNullOrWhiteSpace(settings.HeaderName)
                ? InternalApiOptions.DefaultHeaderName
                : settings.HeaderName;
            request.Headers.TryAddWithoutValidation(headerName, settings.Secret);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
