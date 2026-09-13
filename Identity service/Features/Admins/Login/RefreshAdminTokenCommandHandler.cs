using Identity_service.Contracts.Admins;
using Identity_service.Errors;

namespace Identity_service.Features.Admins.Login;

public sealed class RefreshAdminTokenCommandHandler(
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    UserManager<ApplicationUser> userManager,
    IJwtProvider jwtProvider,
    ILogger<RefreshAdminTokenCommandHandler> logger) : IRequestHandler<RefreshAdminTokenCommand, Result<LoginResponse>>
{
    private const int RefreshTokenExpirationDays = 30;

    public async Task<Result<LoginResponse>> Handle(RefreshAdminTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenProtector.Hash(request.RefreshToken);
        var existing = await unitOfWork.Repository<RefreshToken, Guid>()
            .Query()
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        var user = existing?.User;
        if (existing is null || user is null || !await userManager.IsInRoleAsync(user, DefaultRoles.Admin.Name))
            return Result.Failure<LoginResponse>(UserErrors.InvalidToken);

        if (existing.RevokedAt is not null && existing.ReplacedByTokenId is not null)
        {
            await RevokeTokenFamilyAsync(existing.UserId, existing.FamilyId, cancellationToken);
            logger.LogCritical(
                "Admin refresh token reuse detected for user {UserId} and family {FamilyId}. Token family revoked.",
                existing.UserId,
                existing.FamilyId);
            return Result.Failure<LoginResponse>(UserErrors.InvalidToken);
        }

        if (!existing.IsActive)
            return Result.Failure<LoginResponse>(UserErrors.InvalidToken);

        var replacement = RefreshTokenProtector.Generate();
        var replacementId = Guid.CreateVersion7();
        var refreshExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

        await unitOfWork.Repository<RefreshToken, Guid>().Create(new RefreshToken
        {
            Id = replacementId,
            UserId = existing.UserId,
            TokenHash = RefreshTokenProtector.Hash(replacement),
            FamilyId = existing.FamilyId,
            DeviceInfo = existing.DeviceInfo,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiry
        });

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenId = replacementId;

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresIn) = jwtProvider.GenerateToken(user, roles);
        await unitOfWork.CompleteAsync();

        return Result.Success(new LoginResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            accessToken,
            expiresIn,
            replacement,
            refreshExpiry));
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
