namespace Identity_service.Infrastructure.Implementations.Services;

public sealed class AdminAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        var isAdminEndpoint = policy.Requirements.OfType<RolesAuthorizationRequirement>()
            .Any(requirement => requirement.AllowedRoles.Contains(DefaultRoles.Admin.Name));

        if (!isAdminEndpoint || authorizeResult.Succeeded)
        {
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        await context.RequestServices.GetRequiredService<IAdminSecurityAudit>()
            .AuthorizationFailureAsync(context, context.RequestAborted);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
    }
}
