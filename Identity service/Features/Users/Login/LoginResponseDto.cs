using Identity_service.Entities;

namespace Identity_service.Features.Users.Login;

public sealed record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string Role,
    DriverApplicationStatus? DriverApplicationStatus,
    bool CanAccessDriverHome,
    string? DriverApplicationRejectionReason);
