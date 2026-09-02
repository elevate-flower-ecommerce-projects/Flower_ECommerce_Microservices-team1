using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity_service.Features.Users.Login;

public sealed class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async ([FromBody] LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
            return result.ToHandleResult();
        })
        .AllowAnonymous()
        .RequireRateLimiting("login")
        .WithName("UserLogin")
        .WithTags("Users")
        .ProducesValidationProblem();

        app.MapPost("auth/refresh", async ([FromBody] RefreshTokenRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new RefreshUserTokenCommand(request.RefreshToken), cancellationToken);
            return result.ToHandleResult();
        })
        .AllowAnonymous()
        .WithName("RefreshUserToken")
        .WithTags("Users")
        .ProducesValidationProblem();
    }
}
