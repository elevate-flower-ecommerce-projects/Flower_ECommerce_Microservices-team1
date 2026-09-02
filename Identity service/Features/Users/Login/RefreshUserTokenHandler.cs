using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Errors;
using Identity_service.Infrastructure.Implementations.Services;
using Identity_service.Persistence;
using Identity_service.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Users.Login;

public sealed class RefreshUserTokenHandler(
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    UserManager<ApplicationUser> userManager,
    IDriverLoginStatusGuard driverLoginStatusGuard,
    IJwtTokenService jwtTokenService,
    ILogger<RefreshUserTokenHandler> logger)
    : IRequestHandler<RefreshUserTokenCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(
        RefreshUserTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenProtector.Hash(request.RefreshToken);
        var existing = await unitOfWork.Repository<RefreshToken, Guid>()
            .Query(false)
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        var user = existing?.User;
        if (existing is null || !existing.IsActive || user is null)
        {
            logger.LogWarning("Refresh token request failed: invalid refresh token");
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidToken);
        }

        if (user.IsDisabled)
        {
            logger.LogWarning("Refresh token request blocked for account {UserId}: account is disabled", user.Id);
            return Result.Failure<LoginResponseDto>(UserErrors.AccountDisabled);
        }

        existing.RevokedOn = DateTime.UtcNow;

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var driverAccess = await driverLoginStatusGuard.CheckAsync(user.Id, cancellationToken);

        var tokens = await jwtTokenService.CreateTokensAsync(
            user,
            roles,
            driverAccess.Status,
            cancellationToken);

        logger.LogInformation("Refresh token request succeeded for account {UserId}", user.Id);

        return Result.Success(new LoginResponseDto(
            tokens.AccessToken,
            tokens.RefreshToken,
            Math.Max(0, (int)Math.Ceiling((tokens.AccessTokenExpiresOn - DateTime.UtcNow).TotalSeconds)),
            role,
            driverAccess.Status,
            driverAccess.CanAccessDriverHome,
            driverAccess.RejectionReason));
    }
}
