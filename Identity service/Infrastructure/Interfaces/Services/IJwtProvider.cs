namespace Identity_service.Infrastructure.Interfaces.Services;

public interface IJwtProvider
{
    (string token, int ExpiresOn) GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    string? ValidateToken(string token);
}
