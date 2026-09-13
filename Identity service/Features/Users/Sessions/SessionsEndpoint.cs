using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Users.Sessions;

public sealed class SessionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/sessions", ListSessionsAsync)
            .RequireAuthorization()
            .WithName("ListMySessions")
            .WithTags("Authentication")
            .Produces<OperationResult<IReadOnlyList<SessionResponse>>>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized);

        app.MapDelete("/auth/sessions/{id:guid}", RevokeSessionAsync)
            .RequireAuthorization()
            .WithName("RevokeSession")
            .WithTags("Authentication")
            .Produces<OperationResult>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult>(StatusCodes.Status404NotFound);

        app.MapPost("/auth/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithTags("Authentication")
            .Produces<OperationResult>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> ListSessionsAsync(
        ClaimsPrincipal principal,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
            return UnauthorizedResult();

        var now = DateTime.UtcNow;
        var sessions = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId && token.RevokedAt == null && token.ExpiresAt > now)
            .OrderByDescending(token => token.IssuedAt)
            .Select(token => new SessionResponse(
                token.Id,
                ResolveDeviceName(token.DeviceInfo),
                token.IssuedAt,
                ResolveLocationOrIp(token.DeviceInfo),
                token.ExpiresAt))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<SessionResponse>>(sessions).ToHttpResult();
    }

    private static async Task<IResult> RevokeSessionAsync(
        Guid id,
        ClaimsPrincipal principal,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
            return UnauthorizedResult();

        var session = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.Id == id && token.UserId == userId, cancellationToken);

        if (session is null)
            return OperationResultFactory.NotFound("Session was not found.", "Session was not found.").ToHttpResult();

        session.RevokedAt ??= DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResultFactory.Success("Session revoked successfully.", "Session revoked successfully.").ToHttpResult();
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        ClaimsPrincipal principal,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
            return UnauthorizedResult();

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return OperationResultFactory.BadRequest("Refresh token is required.", "Refresh token is required.").ToHttpResult();

        var tokenHash = RefreshTokenProtector.Hash(request.RefreshToken);
        var session = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.UserId == userId && token.TokenHash == tokenHash, cancellationToken);

        if (session is not null)
        {
            session.RevokedAt ??= DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return OperationResultFactory.Success("Logged out successfully.", "Logged out successfully.").ToHttpResult();
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out string userId)
    {
        userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(userId);
    }

    private static string ResolveDeviceName(string? deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(deviceInfo))
            return "Unknown device";

        var ipStart = deviceInfo.LastIndexOf(" (", StringComparison.Ordinal);
        return ipStart > 0 ? deviceInfo[..ipStart] : deviceInfo;
    }

    private static string? ResolveLocationOrIp(string? deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(deviceInfo))
            return null;

        var ipStart = deviceInfo.LastIndexOf(" (", StringComparison.Ordinal);
        return ipStart >= 0 && deviceInfo.EndsWith(')')
            ? deviceInfo[(ipStart + 2)..^1]
            : null;
    }

    private static IResult UnauthorizedResult()
        => OperationResultFactory.UnAuthorized("Missing user identity.", "Missing user identity.").ToHttpResult();
}
