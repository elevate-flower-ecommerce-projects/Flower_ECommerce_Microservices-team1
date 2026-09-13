namespace Identity_service.Features.Users.Sessions;

public sealed record SessionResponse(
    Guid Id,
    string DeviceName,
    DateTime LastActiveAt,
    string? ApproximateLocationOrIp,
    DateTime ExpiresAt);
