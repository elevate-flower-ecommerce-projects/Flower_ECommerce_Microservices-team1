using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity_service.Features.Users.Login;

public sealed class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async ([FromBody] LoginRequest request, HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new LoginCommand(request.Email, request.Password, BuildDeviceInfo(httpContext)), cancellationToken);
            return result.ToHandleResult();
        })
        .AllowAnonymous()
        .RequireRateLimiting("login")
        .WithName("UserLogin")
        .WithTags("Users")
        .ProducesValidationProblem();

        app.MapPost("auth/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("RefreshUserToken")
            .WithTags("Users")
            .ProducesValidationProblem();

        app.MapPost("auth/refresh-token", RefreshAsync)
            .AllowAnonymous()
            .WithName("RefreshUserTokenV2")
            .WithTags("Users")
            .ProducesValidationProblem();
    }

    private static async Task<IResult> RefreshAsync(
        [FromBody] RefreshTokenRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefreshUserTokenCommand(request.RefreshToken, BuildDeviceInfo(httpContext)), cancellationToken);
        return result.ToHandleResult();
    }

    private static string BuildDeviceInfo(HttpContext httpContext)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(userAgent))
            return ipAddress ?? "Unknown device";

        return string.IsNullOrWhiteSpace(ipAddress) ? userAgent : $"{userAgent} ({ipAddress})";
    }
}
