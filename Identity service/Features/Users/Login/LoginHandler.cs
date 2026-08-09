using System.ComponentModel.DataAnnotations;
using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Errors;
using Identity_service.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity_service.Features.Users.Login;

public sealed class LoginHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IDriverLoginStatusGuard driverLoginStatusGuard,
    IJwtTokenService jwtTokenService,
    ILogger<LoginHandler> logger)
    : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        if (!new EmailAddressAttribute().IsValid(request.Email))
        {
            logger.LogWarning("Login failed: invalid email format");
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidEmailFormat);
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            logger.LogWarning("Login failed: password was not supplied");
            return InvalidCredentials();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        logger.LogInformation("Login attempt for account {Email}", email);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            logger.LogWarning("Login failed for account {Email}: invalid credentials", email);
            return InvalidCredentials();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            logger.LogWarning("Login blocked for account {UserId}: account is locked", user.Id);
            return Result.Failure<LoginResponseDto>(UserErrors.AccountLocked);
        }

        if (!signInResult.Succeeded)
        {
            logger.LogWarning("Login failed for account {UserId}: invalid credentials", user.Id);
            return InvalidCredentials();
        }

        if (user.IsDisabled)
        {
            logger.LogWarning("Login blocked for account {UserId}: account is disabled", user.Id);
            return Result.Failure<LoginResponseDto>(UserErrors.AccountDisabled);
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var driverAccess = await driverLoginStatusGuard.CheckAsync(user.Id, cancellationToken);

        var tokens = await jwtTokenService.CreateTokensAsync(
            user,
            roles,
            driverAccess.Status,
            cancellationToken);

        logger.LogInformation("Login succeeded for account {UserId} with role {Role}", user.Id, role);

        return Result.Success(new LoginResponseDto(
            tokens.AccessToken,
            tokens.RefreshToken,
            Math.Max(0, (int)Math.Ceiling((tokens.AccessTokenExpiresOn - DateTime.UtcNow).TotalSeconds)),
            role,
            driverAccess.Status,
            driverAccess.CanAccessDriverHome,
            driverAccess.RejectionReason));
    }

    private static Result<LoginResponseDto> InvalidCredentials()
        => Result.Failure<LoginResponseDto>(UserErrors.InvalidCredentials);
}
