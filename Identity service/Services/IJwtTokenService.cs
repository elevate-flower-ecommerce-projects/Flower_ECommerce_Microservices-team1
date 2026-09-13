using Identity_service.Entities;

namespace Identity_service.Services;

public sealed record JwtTokenPair(
    string AccessToken,
    DateTime AccessTokenExpiresOn,
    string RefreshToken,
    DateTime RefreshTokenExpiresOn,
    Guid RefreshTokenId);

public interface IJwtTokenService
{
    Task<JwtTokenPair> CreateTokensAsync(
        ApplicationUser user,
        IEnumerable<string> roles,
        DriverApplicationStatus? driverApplicationStatus,
        Guid? refreshTokenFamilyId,
        string? deviceInfo,
        CancellationToken cancellationToken);
}
