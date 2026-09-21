using System.Security.Cryptography;
using System.Text;
using Flower.Common.StandardizedResponse;
using Microsoft.Extensions.Options;
using Order___Fulfillment_Service.Settings;

namespace Order___Fulfillment_Service.Features.Internal;

public static class InternalRoutes
{
    // The gateway strips "/api/v1/orders", so these live under /internal on this service.
    public const string Order = "/internal/orders/{orderId:guid}";
    public const string OrderPaymentSucceeded = "/internal/orders/{orderId:guid}/payment-succeeded";
}

public static class InternalMessages
{
    public const string NotInternalCaller = "This endpoint is only available to Flower services.";
    public const string SecretNotConfigured = "Internal endpoints are disabled because no internal secret is configured.";
    public const string OrderLoaded = "Order loaded successfully.";
    public const string OrderMarkedPaid = "Order was marked as paid.";
    public const string OrderAlreadyPaid = "Order was already paid.";
    public const string OrderNotPayable = "This order cannot be marked as paid.";
}

/// <summary>
/// Rejects anything that does not present the shared internal secret. It fails closed: if no secret
/// is configured, every internal call is refused rather than left open.
/// </summary>
public sealed class InternalOnlyFilter(
    IOptions<InternalApiOptions> options,
    ILogger<InternalOnlyFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Secret))
        {
            logger.LogError("An internal endpoint was called but Internal:Secret is not configured.");
            return OperationResultFactory
                .UnAuthorized(InternalMessages.SecretNotConfigured, InternalMessages.SecretNotConfigured)
                .ToHttpResult();
        }

        var headerName = string.IsNullOrWhiteSpace(settings.HeaderName)
            ? InternalApiOptions.DefaultHeaderName
            : settings.HeaderName;
        var presented = context.HttpContext.Request.Headers[headerName].ToString();

        if (!MatchesSecret(presented, settings.Secret))
        {
            logger.LogWarning(
                "Rejected an internal call to {Path} without a valid {HeaderName} header.",
                context.HttpContext.Request.Path,
                headerName);
            return OperationResultFactory
                .UnAuthorized(InternalMessages.NotInternalCaller, InternalMessages.NotInternalCaller)
                .ToHttpResult();
        }

        return await next(context);
    }

    // Compared in constant time so the response time cannot be used to guess the secret byte by byte.
    private static bool MatchesSecret(string presented, string expected)
    {
        if (string.IsNullOrEmpty(presented))
            return false;

        var presentedBytes = Encoding.UTF8.GetBytes(presented);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return CryptographicOperations.FixedTimeEquals(presentedBytes, expectedBytes);
    }
}
