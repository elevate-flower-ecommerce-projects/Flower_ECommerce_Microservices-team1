using Identity_service.Contracts.Admins;
using Identity_service.Errors;
using Identity_service.Infrastructure.Interfaces.Services;

namespace Identity_service.Features.Admins.Login;

public class AdminLoginCommandHandler(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider,
    SignInManager<ApplicationUser> signInManager, IUnitOfWork<ApplicationDbContext> unitOfWork,
    IAdminLoginAttemptGuard attemptGuard, IAdminSecurityAudit audit)
    : IRequestHandler<AdminLoginCommand, Result<LoginResponse>>
{
    private readonly int _refreshTokenExpirationDays = 7;
    public async Task<Result<LoginResponse>> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        if (attemptGuard.IsBlocked(request.Email, request.IpAddress))
        {
            await audit.LoginAttemptAsync(request.Email, request.IpAddress, request.UserAgent, "RateLimited", cancellationToken);
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);
        }

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return await FailedAsync("Failed", request, cancellationToken);

        var isAdminRole = await userManager.IsInRoleAsync(user, DefaultRoles.Admin.Name);

        if (!isAdminRole)
            return await FailedAsync("Failed", request, cancellationToken);

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, true);

        if (!result.Succeeded)
            return await FailedAsync(result.IsLockedOut ? "LockedOut" : "Failed", request, cancellationToken);

        var userRoles = await userManager.GetRolesAsync(user);

        var (token, ExpiresIn) = jwtProvider.GenerateToken(user, userRoles);

        var refreshToken = RefreshTokenProtector.Generate();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

        var refreshTokenEntity = new RefreshToken
        {
            TokenHash = RefreshTokenProtector.Hash(refreshToken),
            ExpiresOn = refreshTokenExpiration,
            UserId = user.Id
        };

        await unitOfWork.Repository<RefreshToken, Guid>().Create(refreshTokenEntity);

        await unitOfWork.CompleteAsync();

        attemptGuard.ResetAccount(request.Email);
        await audit.LoginAttemptAsync(request.Email, request.IpAddress, request.UserAgent, "Succeeded", cancellationToken);

        var response = new LoginResponse(user.Id, user.FirstName, user.LastName, user.Email!, token, ExpiresIn, refreshToken, refreshTokenExpiration);

        return Result.Success(response);
    }

    private async Task<Result<LoginResponse>> FailedAsync(string outcome, AdminLoginCommand request, CancellationToken cancellationToken)
    {
        attemptGuard.RegisterFailure(request.Email, request.IpAddress);
        await audit.LoginAttemptAsync(request.Email, request.IpAddress, request.UserAgent, outcome, cancellationToken);
        return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);
    }
}
