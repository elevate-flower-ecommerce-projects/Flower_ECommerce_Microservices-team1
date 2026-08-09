namespace Identity_service.Infrastructure.Interfaces.Services;

public interface IAdminSecurityAudit
{
    Task LoginAttemptAsync(string email, string? ipAddress, string? userAgent, string outcome, CancellationToken cancellationToken);
    Task AuthorizationFailureAsync(HttpContext context, CancellationToken cancellationToken);
}
