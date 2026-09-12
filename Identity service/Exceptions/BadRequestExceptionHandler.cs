using Flower.Common.StandardizedResponse;
using Microsoft.AspNetCore.Diagnostics;

namespace Identity_service.Exceptions;

/// <summary>
/// Turns request binding failures (for example an unknown enum value such as vehicleType=Truck or
/// a malformed body) into a 400 instead of letting GlobalExceptionHandler report them as a 500.
/// </summary>
internal sealed class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequest)
            return false;

        logger.LogWarning(exception, "Rejected a malformed request: {Message}", badRequest.Message);

        httpContext.Response.StatusCode = badRequest.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new OperationResult(
                (Flower.Common.StandardizedResponse.StatusCode)badRequest.StatusCode,
                badRequest.Message,
                badRequest.Message),
            cancellationToken);

        return true;
    }
}
