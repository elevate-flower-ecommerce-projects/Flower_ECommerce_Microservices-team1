using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity_service.Entities;
using Identity_service.Persistence;
using Identity_service.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repository.Layer.Interfaces;

namespace Identity_service.Services;

public sealed class JwtTokenService(
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    IOptions<JwtOptions> jwtOptions) : IJwtTokenService
{
    public async Task<JwtTokenPair> CreateTokensAsync(
        ApplicationUser user,
        IEnumerable<string> roles,
        DriverApplicationStatus? driverApplicationStatus,
        CancellationToken cancellationToken)
    {
        var options = jwtOptions.Value;
        var now = DateTime.UtcNow;
        var accessTokenExpiresOn = now.AddMinutes(options.ExpiryMinutes);
        var refreshTokenExpiresOn = now.AddDays(options.RefreshTokenExpiryDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (driverApplicationStatus is not null)
        {
            claims.Add(new Claim(
                "driver_application_status",
                driverApplicationStatus.Value.ToString()));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var jwt = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiresOn,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        await unitOfWork.Repository<RefreshToken, Guid>().Create(new RefreshToken
        {
            TokenHash = refreshTokenHash,
            ExpiresOn = refreshTokenExpiresOn,
            UserId = user.Id
        });
        await unitOfWork.CompleteAsync();

        return new JwtTokenPair(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            accessTokenExpiresOn,
            refreshToken,
            refreshTokenExpiresOn);
    }
}
