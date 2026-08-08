using Identity_service.Contracts.Admins;
using Identity_service.Errors;

namespace Identity_service.Features.Admins.Login;

public sealed class RefreshAdminTokenCommandHandler(
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    UserManager<ApplicationUser> userManager,
    IJwtProvider jwtProvider) : IRequestHandler<RefreshAdminTokenCommand, Result<LoginResponse>>
{
    private const int RefreshTokenExpirationDays = 7;

    public async Task<Result<LoginResponse>> Handle(RefreshAdminTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenProtector.Hash(request.RefreshToken);
        var existing = await unitOfWork.Repository<RefreshToken, Guid>()
            .Query()
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        var user = existing?.User;
        if (existing is null || !existing.IsActive || user is null ||
            !await userManager.IsInRoleAsync(user, DefaultRoles.Admin.Name))
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);

        existing.RevokedOn = DateTime.UtcNow;
        var replacement = RefreshTokenProtector.Generate();
        var refreshExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);
        await unitOfWork.Repository<RefreshToken, Guid>().Create(new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = RefreshTokenProtector.Hash(replacement),
            ExpiresOn = refreshExpiry
        });

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
}
