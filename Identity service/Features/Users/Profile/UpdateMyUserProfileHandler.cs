using Flower.Common.StandardizedResponse;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Identity_service.Features.Users.Profile;

public sealed partial class UpdateMyUserProfileHandler(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ILogger<UpdateMyUserProfileHandler> logger)
    : IRequestHandler<UpdateMyUserProfileCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        UpdateMyUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            return Validation(errors);

        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return OperationResultFactory.NotFound<object>(
                message: "User profile was not found.",
                messageLocalized: "User profile was not found.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var phoneNumber = request.PhoneNumber.Trim();

        if (await dbContext.Users.AnyAsync(candidate => candidate.Id != user.Id && candidate.NormalizedEmail == email.ToUpper(), cancellationToken))
            errors[nameof(request.Email)] = ["Email already registered"];

        if (await dbContext.Users.AnyAsync(candidate => candidate.Id != user.Id && candidate.PhoneNumber == phoneNumber, cancellationToken))
            errors[nameof(request.PhoneNumber)] = ["Phone number already registered"];

        if (errors.Count > 0)
            return Validation(errors);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UserName = email;
        user.Email = email;
        user.PhoneNumber = phoneNumber;
        user.ProfilePictureUrl = string.IsNullOrWhiteSpace(request.ProfilePictureUrl) ? null : request.ProfilePictureUrl.Trim();
        _ = Enum.TryParse<Gender>(request.Gender.Trim(), ignoreCase: true, out var gender);
        user.Gender = gender;

        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            logger.LogWarning("Failed to update user profile {UserId}. Identity errors: {Errors}", user.Id, string.Join(", ", updated.Errors.Select(error => error.Code)));
            foreach (var error in updated.Errors)
            {
                var field = error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase) || error.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase)
                    ? nameof(request.Email)
                    : "Profile";

                errors[field] = errors.TryGetValue(field, out var existing) ? [.. existing, error.Description] : [error.Description];
            }

            return Validation(errors);
        }

        return OperationResultFactory.Success<object>(GetMyUserProfileHandler.ToResponse(user), "Profile updated successfully.", "Profile updated successfully.");
    }

    private static Dictionary<string, string[]> Validate(UpdateMyUserProfileCommand request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var phoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
        var gender = request.Gender?.Trim() ?? string.Empty;
        var profilePictureUrl = request.ProfilePictureUrl?.Trim();

        AddIf(errors, nameof(request.FirstName), firstName.Length == 0, "First name is required.");
        AddIf(errors, nameof(request.FirstName), firstName.Length > 100, "First name must not exceed 100 characters.");
        AddIf(errors, nameof(request.LastName), lastName.Length == 0, "Last name is required.");
        AddIf(errors, nameof(request.LastName), lastName.Length > 100, "Last name must not exceed 100 characters.");
        AddIf(errors, nameof(request.Email), email.Length == 0, "Email is required.");
        AddIf(errors, nameof(request.Email), email.Length > 0 && (email.Length > 256 || !new EmailAddressAttribute().IsValid(email)), "Enter a valid email address.");
        AddIf(errors, nameof(request.PhoneNumber), phoneNumber.Length == 0, "Phone number is required.");
        AddIf(errors, nameof(request.PhoneNumber), phoneNumber.Length > 0 && !EgyptianMobileRegex().IsMatch(phoneNumber), "Enter a valid Egyptian mobile number (01[0-2,5]XXXXXXXX).");
        AddIf(errors, nameof(request.Gender), gender.Length == 0, "Gender is required.");
        AddIf(errors, nameof(request.Gender), gender.Length > 0 && !IsSupportedGender(gender), "Gender must be Male or Female.");
        AddIf(errors, nameof(request.ProfilePictureUrl), profilePictureUrl?.Length > 512, "Profile picture URL must not exceed 512 characters.");

        return errors;
    }

    private static OperationResult<object> Validation(Dictionary<string, string[]> errors)
        => OperationResultFactory.Validation<object>(errors, "Profile validation failed.", "Profile validation failed.");

    private static bool IsSupportedGender(string value)
        => Enum.TryParse<Gender>(value, ignoreCase: true, out var gender) && Enum.IsDefined(gender);

    private static void AddIf(Dictionary<string, string[]> errors, string field, bool condition, string message)
    {
        if (!condition)
            return;

        errors[field] = errors.TryGetValue(field, out var current) ? [.. current, message] : [message];
    }

    [GeneratedRegex(@"^01[0125]\d{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex EgyptianMobileRegex();
}
