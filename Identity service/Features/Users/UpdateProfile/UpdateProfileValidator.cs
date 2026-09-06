using Identity_service.Persistence;
using Identity_service.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity_service.Features.Users.UpdateProfile;

/// <summary>
/// Applies the registration field rules to a profile edit, with one required difference: the
/// uniqueness checks skip the caller's own row, otherwise saving the form without touching the
/// email would report the user's own address as a duplicate.
/// </summary>
public sealed class UpdateProfileValidator(
    ApplicationDbContext dbContext,
    IOptions<AvatarStorageOptions> avatarOptions) : IUpdateProfileValidator
{
    public async Task<Dictionary<string, string[]>> ValidateAsync(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var phoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
        var gender = request.Gender?.Trim() ?? string.Empty;

        #region Required fields and formats

        UserProfileFieldRules.AddIf(errors, nameof(request.FullName), fullName.Length == 0, "Full name is required.");

        if (fullName.Length > 0)
        {
            var name = UserProfileFieldRules.SplitFullName(fullName);
            UserProfileFieldRules.AddIf(
                errors,
                nameof(request.FullName),
                name.FirstName.Length > UserProfileFieldRules.MaxNamePartLength
                    || name.LastName.Length > UserProfileFieldRules.MaxNamePartLength,
                "First name and last name must not exceed 100 characters each.");
        }

        UserProfileFieldRules.AddIf(errors, nameof(request.Email), email.Length == 0, "Email is required.");
        UserProfileFieldRules.AddIf(
            errors,
            nameof(request.Email),
            email.Length > 0 && !UserProfileFieldRules.IsValidEmail(email),
            "Enter a valid email address.");

        UserProfileFieldRules.AddIf(errors, nameof(request.PhoneNumber), phoneNumber.Length == 0, "Phone number is required.");
        UserProfileFieldRules.AddIf(
            errors,
            nameof(request.PhoneNumber),
            phoneNumber.Length > 0 && !UserProfileFieldRules.IsValidEgyptianMobile(phoneNumber),
            "Enter a valid Egyptian mobile number (01[0-2,5]XXXXXXXX).");

        UserProfileFieldRules.AddIf(errors, nameof(request.Gender), gender.Length == 0, "Gender is required.");
        UserProfileFieldRules.AddIf(
            errors,
            nameof(request.Gender),
            gender.Length > 0 && !UserProfileFieldRules.IsSupportedGender(gender),
            "Gender must be Male or Female.");

        ValidateProfilePicture(request.ProfilePicture, errors);

        #endregion

        // Database checks only run for structurally valid input.
        if (errors.Count > 0)
            return errors;

        #region Duplicate checks, excluding the caller

        var normalizedEmail = email.ToUpperInvariant();

        var emailTaken = await dbContext.Users.AnyAsync(
            user => user.NormalizedEmail == normalizedEmail && user.Id != request.UserId,
            cancellationToken);

        if (emailTaken)
            errors[nameof(request.Email)] = [UpdateProfileMessages.EmailAlreadyRegistered];

        var phoneTaken = await dbContext.Users.AnyAsync(
            user => user.PhoneNumber == phoneNumber && user.Id != request.UserId,
            cancellationToken);

        if (phoneTaken)
            errors[nameof(request.PhoneNumber)] = [UpdateProfileMessages.PhoneNumberAlreadyRegistered];

        #endregion

        return errors;
    }

    private void ValidateProfilePicture(IFormFile? file, Dictionary<string, string[]> errors)
    {
        if (file is null)
            return;

        var options = avatarOptions.Value;
        var allowedContentTypes = options.AllowedContentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        UserProfileFieldRules.AddIf(
            errors,
            nameof(UpdateProfileCommand.ProfilePicture),
            file.Length <= 0,
            "The selected image is empty.");

        UserProfileFieldRules.AddIf(
            errors,
            nameof(UpdateProfileCommand.ProfilePicture),
            file.Length > options.MaxFileSizeBytes,
            $"The image exceeds the {options.MaxFileSizeBytes / (1024 * 1024)}MB size limit.");

        // The declared content type is client-supplied, so the file signature is checked too.
        // Driver documents compare the file extension instead; that cannot be used here because
        // camera captures often arrive with no extension at all.
        UserProfileFieldRules.AddIf(
            errors,
            nameof(UpdateProfileCommand.ProfilePicture),
            file.Length > 0
                && (!allowedContentTypes.Contains(file.ContentType) || !HasImageSignature(file)),
            "The image must be a jpg or png file.");
    }

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    private static bool HasImageSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[8];

        using var stream = file.OpenReadStream();
        var read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

        return (read >= PngSignature.Length && header[..PngSignature.Length].SequenceEqual(PngSignature))
            || (read >= JpegSignature.Length && header[..JpegSignature.Length].SequenceEqual(JpegSignature));
    }
}
