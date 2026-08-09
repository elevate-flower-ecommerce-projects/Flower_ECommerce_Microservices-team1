namespace Identity_service.Infrastructure.Implementations.Services;

public sealed class AdminSecurityAuditWriter(IUnitOfWork<ApplicationDbContext> unitOfWork) : IAdminSecurityAudit
{
    public Task LoginAttemptAsync(string email, string? ipAddress, string? userAgent, string outcome, CancellationToken cancellationToken) =>
        WriteAsync("Login", outcome, email, ipAddress, userAgent, null, cancellationToken);

    public Task AuthorizationFailureAsync(HttpContext context, CancellationToken cancellationToken) =>
        WriteAsync(
            "Authorization",
            "Forbidden",
            context.User.Identity?.Name,
            context.Connection.RemoteIpAddress?.ToString(),
            context.Request.Headers.UserAgent.ToString(),
            context.Request.Path.Value,
            cancellationToken);

    private async Task WriteAsync(string eventType, string outcome, string? email, string? ipAddress, string? userAgent, string? path, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<AdminSecurityAudit, Guid>().Create(new AdminSecurityAudit
        {
            EventType = eventType,
            Outcome = outcome,
            Email = Trim(email, 256),
            IpAddress = Trim(ipAddress, 64),
            UserAgent = Trim(userAgent, 512),
            Path = Trim(path, 512)
        });

        await unitOfWork.CompleteAsync();
    }

    private static string? Trim(string? value, int length) =>
        string.IsNullOrWhiteSpace(value) ? null : value[..Math.Min(value.Length, length)];
}
