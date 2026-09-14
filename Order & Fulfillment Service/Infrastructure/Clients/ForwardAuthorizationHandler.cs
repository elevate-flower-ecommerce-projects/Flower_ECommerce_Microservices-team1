namespace Order___Fulfillment_Service.Infrastructure.Clients;

/// <summary>
/// Checkout reads the customer's own cart and addresses, so each downstream call carries the
/// customer's token and the other services apply their usual ownership rules.
/// </summary>
public sealed class ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization))
            request.Headers.TryAddWithoutValidation("Authorization", authorization);

        return base.SendAsync(request, cancellationToken);
    }
}
