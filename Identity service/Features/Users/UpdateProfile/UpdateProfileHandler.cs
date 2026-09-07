using Flower.Common.StandardizedResponse;
using Identity_service.Abstractions;
using Identity_service.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity_service.Features.Users.UpdateProfile;

public sealed class UpdateProfileHandler(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IUpdateProfileValidator validator,
    IAvatarStorage avatarStorage,
    ILogger<UpdateProfileHandler> logger)
    : IRequestHandler<UpdateProfileCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return OperationResultFactory.NotFound<object>(
                message: UpdateProfileMessages.UserNotFound,
                messageLocalized: UpdateProfileMessages.UserNotFound);
        }

        var validationErrors = await validator.ValidateAsync(request, cancellationToken);
        if (validationErrors.Count > 0)
            return ValidationFailure(validationErrors);

        var email = request.Email.Trim().ToLowerInvariant();
        var phoneNumber = request.PhoneNumber.Trim();
        var name = UserProfileFieldRules.SplitFullName(request.FullName);
        _ = Enum.TryParse<Gender>(request.Gender.Trim(), ignoreCase: true, out var gender);

        var emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
        var phoneChanged = !string.Equals(user.PhoneNumber, phoneNumber, StringComparison.Ordinal);

        var previousAvatarUrl = user.ProfilePictureUrl;
        string? storedAvatarUrl = null;

        try
        {
            if (request.ProfilePicture is not null)
            {
                storedAvatarUrl = await avatarStorage.SaveAsync(user.Id, request.ProfilePicture, cancellationToken);
                user.ProfilePictureUrl = storedAvatarUrl;
            }

            user.FirstName = name.FirstName;
            user.LastName = name.LastName;
            user.Gender = gender;

            if (emailChanged)
            {
                // The email doubles as the username, so both have to move together or the next
                // login with the new address fails.
                var setEmail = await userManager.SetEmailAsync(user, email);
                if (!setEmail.Succeeded)
                    return await RollbackAsync(storedAvatarUrl, MapIdentityErrors(setEmail), cancellationToken);

                var setUserName = await userManager.SetUserNameAsync(user, email);
                if (!setUserName.Succeeded)
                    return await RollbackAsync(storedAvatarUrl, MapIdentityErrors(setUserName), cancellationToken);
            }

            if (phoneChanged)
            {
                var setPhone = await userManager.SetPhoneNumberAsync(user, phoneNumber);
                if (!setPhone.Succeeded)
                    return await RollbackAsync(storedAvatarUrl, MapIdentityErrors(setPhone), cancellationToken);
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return await RollbackAsync(storedAvatarUrl, MapIdentityErrors(updateResult), cancellationToken);

            if (emailChanged)
                await RevokeActiveRefreshTokensAsync(user.Id, cancellationToken);

            // Only drop the previous file once the new one is safely persisted.
            if (storedAvatarUrl is not null)
                await avatarStorage.DeleteAsync(previousAvatarUrl, cancellationToken);

            var roles = await userManager.GetRolesAsync(user);

            var message = emailChanged
                ? UpdateProfileMessages.EmailChangedSignOut
                : UpdateProfileMessages.ProfileUpdated;

            return OperationResultFactory.Success<object>(
                user.ToProfileResponse(roles, emailChanged),
                message,
                message);
        }
        catch (DbUpdateException exception) when (TryMapDuplicateError(exception, out var duplicateErrors))
        {
            // The unique index is the backstop when two requests race past the checks above.
            logger.LogWarning(exception, "A duplicate email or phone was detected while updating {UserId}.", request.UserId);
            return await RollbackAsync(storedAvatarUrl, duplicateErrors, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Profile update failed for {UserId}.", request.UserId);
            await avatarStorage.DeleteAsync(storedAvatarUrl, CancellationToken.None);

            return OperationResultFactory.Error<object>(
                message: UpdateProfileMessages.UpdateFailed,
                messageLocalized: UpdateProfileMessages.UpdateFailed);
        }
    }

    /// <summary>
    /// Changing the email is a credential change, so existing sessions must not be extendable.
    /// Everything else leaves the session alone so the app reflects the change without a re-login.
    /// </summary>
    private async Task RevokeActiveRefreshTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.Set<RefreshToken>()
            .Where(token => token.UserId == userId && token.RevokedOn == null)
            .ToListAsync(cancellationToken);

        if (activeTokens.Count == 0)
            return;

        foreach (var token in activeTokens)
            token.RevokedOn = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Revoked {Count} refresh tokens after an email change for {UserId}.", activeTokens.Count, userId);
    }

    /// <summary>Discards a just-uploaded avatar when the rest of the update could not be saved.</summary>
    private async Task<OperationResult<object>> RollbackAsync(
        string? storedAvatarUrl,
        Dictionary<string, string[]> errors,
        CancellationToken cancellationToken)
    {
        await avatarStorage.DeleteAsync(storedAvatarUrl, cancellationToken);
        return ValidationFailure(errors);
    }

    private static OperationResult<object> ValidationFailure(Dictionary<string, string[]> errors)
        => OperationResultFactory.Validation<object>(
            errors,
            UpdateProfileMessages.ValidationFailed,
            UpdateProfileMessages.ValidationFailed);

    private static Dictionary<string, string[]> MapIdentityErrors(IdentityResult result)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var error in result.Errors)
        {
            var field = error.Code.Contains("Phone", StringComparison.OrdinalIgnoreCase)
                ? nameof(UpdateProfileCommand.PhoneNumber)
                : error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase)
                    || error.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase)
                    ? nameof(UpdateProfileCommand.Email)
                    : "Profile";

            var message = field == nameof(UpdateProfileCommand.Email)
                && error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)
                    ? UpdateProfileMessages.EmailAlreadyRegistered
                    : error.Description;

            errors[field] = errors.TryGetValue(field, out var current) ? [.. current, message] : [message];
        }

        return errors;
    }

    private static bool TryMapDuplicateError(DbUpdateException exception, out Dictionary<string, string[]> errors)
    {
        var databaseMessage = exception.InnerException?.Message ?? exception.Message;

        if (databaseMessage.Contains("UX_ApplicationUser_PhoneNumber", StringComparison.OrdinalIgnoreCase))
        {
            errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(UpdateProfileCommand.PhoneNumber)] = [UpdateProfileMessages.PhoneNumberAlreadyRegistered]
            };
            return true;
        }

        if (databaseMessage.Contains("UserNameIndex", StringComparison.OrdinalIgnoreCase)
            || databaseMessage.Contains("NormalizedUserName", StringComparison.OrdinalIgnoreCase)
            || databaseMessage.Contains("EmailIndex", StringComparison.OrdinalIgnoreCase))
        {
            errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(UpdateProfileCommand.Email)] = [UpdateProfileMessages.EmailAlreadyRegistered]
            };
            return true;
        }

        errors = [];
        return false;
    }
}
