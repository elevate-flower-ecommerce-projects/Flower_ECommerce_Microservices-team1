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
            .Query()
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        var user = existing?.User;
        if (existing is null || user is null)
        {
            logger.LogWarning("Refresh token request failed: invalid refresh token");
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidToken);
        }

        if (existing.RevokedAt is not null && existing.ReplacedByTokenId is not null)
        {
            await RevokeTokenFamilyAsync(existing.UserId, existing.FamilyId, cancellationToken);
            logger.LogCritical(
                "Refresh token reuse detected for user {UserId} and family {FamilyId}. Token family revoked.",
                existing.UserId,
                existing.FamilyId);
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidToken);
        }

        if (!existing.IsActive)
        {
            logger.LogWarning("Refresh token request failed for account {UserId}: inactive refresh token", existing.UserId);
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidToken);
        }

        if (user.IsDisabled)
        {
            logger.LogWarning("Refresh token request blocked for account {UserId}: account is disabled", user.Id);
            return Result.Failure<LoginResponseDto>(UserErrors.AccountDisabled);
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var driverAccess = await driverLoginStatusGuard.CheckAsync(user.Id, cancellationToken);

        var tokens = await jwtTokenService.CreateTokensAsync(
            user,
            roles,
            driverAccess.Status,
            existing.FamilyId,
            request.DeviceInfo ?? existing.DeviceInfo,
            cancellationToken);

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenId = tokens.RefreshTokenId;
        await unitOfWork.CompleteAsync();

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

    private async Task RevokeTokenFamilyAsync(string userId, Guid familyId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var familyTokens = await unitOfWork.Repository<RefreshToken, Guid>()
            .Query()
            .Where(token => token.UserId == userId && token.FamilyId == familyId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in familyTokens)
            token.RevokedAt = now;

        await unitOfWork.CompleteAsync();
    }
}
