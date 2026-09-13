using Flower.Common.StandardizedResponse;
using Identity_service.Persistence;
using Identity_service.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity_service.Features.Users.ChangePassword;

public sealed class ChangePasswordHandler(
    UserManager<ApplicationUser> userManager,
    IPasswordValidator<ApplicationUser> passwordValidator,
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    PasswordChangedEmailService passwordChangedEmailService,
    ILogger<ChangePasswordHandler> logger)
    : IRequestHandler<ChangePasswordCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return OperationResultFactory.NotFound<object>(
                message: ChangePasswordMessages.UserNotFound,
                messageLocalized: ChangePasswordMessages.UserNotFound);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword)
            || !await userManager.CheckPasswordAsync(user, request.CurrentPassword))
        {
            logger.LogWarning("Password change rejected for user {UserId}: current password was incorrect.", request.UserId);
            return OperationResultFactory.UnAuthorized<object>(
                data: null!,
                message: ChangePasswordMessages.CurrentPasswordIncorrect,
                messageLocalized: ChangePasswordMessages.CurrentPasswordIncorrect);
        }

        var validationErrors = await ValidateNewPasswordAsync(user, request);
        if (validationErrors.Count > 0)
            return ValidationFailure(validationErrors);

        var changePassword = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changePassword.Succeeded)
            return ValidationFailure(MapIdentityErrors(changePassword));

        await userManager.UpdateSecurityStampAsync(user);
        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.Email))
            await passwordChangedEmailService.SendAsync(user.Email, cancellationToken);

        logger.LogInformation("Password changed and all refresh tokens revoked for user {UserId}.", user.Id);

        return OperationResultFactory.Success<object>(
            new { signOut = true, redirectTo = "/login" },
            ChangePasswordMessages.PasswordChanged,
            ChangePasswordMessages.PasswordChanged);
    }

    private async Task<Dictionary<string, string[]>> ValidateNewPasswordAsync(
        ApplicationUser user,
        ChangePasswordCommand request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        AddIf(errors, nameof(request.NewPassword), string.IsNullOrWhiteSpace(request.NewPassword), "New password is required.");
        AddIf(errors, nameof(request.ConfirmNewPassword), string.IsNullOrWhiteSpace(request.ConfirmNewPassword), "Confirm new password is required.");
        AddIf(
            errors,
            nameof(request.ConfirmNewPassword),
            !string.IsNullOrEmpty(request.ConfirmNewPassword)
                && !string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal),
            "Confirm new password must exactly match new password.");

        if (errors.Count > 0)
            return errors;

        if (await userManager.CheckPasswordAsync(user, request.NewPassword))
        {
            errors[nameof(request.NewPassword)] = [ChangePasswordMessages.NewPasswordSameAsCurrent];
            return errors;
        }

        var validation = await passwordValidator.ValidateAsync(userManager, user, request.NewPassword);
        if (!validation.Succeeded)
            return MapIdentityErrors(validation);

        return errors;
    }

    private async Task RevokeAllRefreshTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var activeTokens = await unitOfWork.Repository<RefreshToken, Guid>()
            .Query()
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.RevokedAt = now;

        await unitOfWork.CompleteAsync();
    }

    private static OperationResult<object> ValidationFailure(Dictionary<string, string[]> errors)
        => OperationResultFactory.Validation<object>(
            errors,
            ChangePasswordMessages.ValidationFailed,
            ChangePasswordMessages.ValidationFailed);

    private static Dictionary<string, string[]> MapIdentityErrors(IdentityResult result)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var error in result.Errors)
        {
            var field = error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)
                ? nameof(ChangePasswordCommand.NewPassword)
                : "Password";

            errors[field] = errors.TryGetValue(field, out var current)
                ? [.. current, error.Description]
                : [error.Description];
        }

        return errors;
    }

    private static void AddIf(Dictionary<string, string[]> errors, string field, bool condition, string message)
    {
        if (!condition)
            return;

        errors[field] = errors.TryGetValue(field, out var current)
            ? [.. current, message]
            : [message];
    }
}
