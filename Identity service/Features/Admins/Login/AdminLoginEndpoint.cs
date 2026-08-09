using Identity_service.Contracts.Admins;
using Microsoft.AspNetCore.Mvc;

namespace Identity_service.Features.Admins.Login;

public class AdminLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/admin/login", async (HttpContext httpContext, [FromBody] LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new AdminLoginCommand(request.Email, request.Password,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString());

            var result = await sender.Send(command, cancellationToken);

            return result.ToHandleResult();
        })
        .AllowAnonymous()
        .WithName("AdminLogin")
        .WithTags("Admins")
        .ProducesValidationProblem();

        app.MapPost("api/admin/refresh", async ([FromBody] RefreshTokenRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new RefreshAdminTokenCommand(request.RefreshToken), cancellationToken);
            return result.ToHandleResult();
        })
        .AllowAnonymous()
        .WithName("RefreshAdminToken")
        .WithTags("Admins");

        app.MapGet("api/admin/session", () => Results.NoContent())
            .RequireAuthorization(policy =>
                policy.RequireRole(DefaultRoles.Admin.Name))
            .WithName("AdminSession")
            .WithTags("Admins");
    }
}
