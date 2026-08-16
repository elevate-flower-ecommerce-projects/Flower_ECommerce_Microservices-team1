using Identity_service.Entities;
using Identity_service.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Identity_service.Features.Users.Register;

/// <summary>
/// Validates customer registration input and maps failures to request field names.
/// </summary>
public sealed partial class RegisterCustomerValidator(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) : IRegisterCustomerValidator
{
    public async Task<Dictionary<string, string[]>> ValidateAsync(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var phoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
        var gender = request.Gender?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var confirmPassword = request.ConfirmPassword ?? string.Empty;

        #region Required fields and formats

        AddIf(errors, nameof(request.FullName), fullName.Length == 0, "Full name is required.");

        if (fullName.Length > 0)
        {
            var name = SplitFullName(fullName);
            AddIf(
                errors,
                nameof(request.FullName),
                name.FirstName.Length > 100 || name.LastName.Length > 100,
                "First name and last name must not exceed 100 characters each.");
        }

        AddIf(errors, nameof(request.Email), email.Length == 0, "Email is required.");
        AddIf(
            errors,
            nameof(request.Email),
            email.Length > 0 && (email.Length > 256 || !new EmailAddressAttribute().IsValid(email)),
            "Enter a valid email address.");

        AddIf(errors, nameof(request.PhoneNumber), phoneNumber.Length == 0, "Phone number is required.");
        AddIf(
            errors,
            nameof(request.PhoneNumber),
            phoneNumber.Length > 0 && !EgyptianMobileRegex().IsMatch(phoneNumber),
            "Enter a valid Egyptian mobile number (01[0-2,5]XXXXXXXX).");

        AddIf(errors, nameof(request.Gender), gender.Length == 0, "Gender is required.");
        AddIf(
            errors,
            nameof(request.Gender),
            gender.Length > 0 && !IsSupportedGender(gender),
            "Gender must be Male or Female.");

        AddIf(errors, nameof(request.Password), password.Length == 0, "Password is required.");
        AddIf(
            errors,
            nameof(request.Password),
            password.Length > 0 && !PasswordRegex().IsMatch(password),
            "Password must be at least 6 characters and contain at least one uppercase letter and one digit.");

        AddIf(errors, nameof(request.ConfirmPassword), confirmPassword.Length == 0, "Confirm password is required.");
        AddIf(
            errors,
            nameof(request.ConfirmPassword),
            confirmPassword.Length > 0 && !string.Equals(password, confirmPassword, StringComparison.Ordinal),
            "Confirm password must exactly match password.");

        #endregion

        // Database checks only run for structurally valid input.
        if (errors.Count > 0)
            return errors;

        #region Duplicate checks

        if (await userManager.FindByEmailAsync(email) is not null)
            errors[nameof(request.Email)] = [RegisterCustomerMessages.EmailAlreadyRegistered];

        if (await dbContext.Users.AnyAsync(
            user => user.PhoneNumber == phoneNumber,
            cancellationToken))
        {
            errors[nameof(request.PhoneNumber)] = [RegisterCustomerMessages.PhoneNumberAlreadyRegistered];
        }

        #endregion

        return errors;
    }

    private static bool IsSupportedGender(string value)
        => Enum.TryParse<Gender>(value, ignoreCase: true, out var gender)
            && Enum.IsDefined(gender);

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1
            ? (parts[0], parts[0])
            : (parts[0], parts[1]);
    }

    private static void AddIf(
        Dictionary<string, string[]> errors,
        string field,
        bool condition,
        string message)
    {
        if (!condition)
            return;

        errors[field] = errors.TryGetValue(field, out var current)
            ? [.. current, message]
            : [message];
    }

    [GeneratedRegex(@"^01[0125]\d{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex EgyptianMobileRegex();

    [GeneratedRegex(@"^(?=.*[A-Z])(?=.*\d).{6,}$", RegexOptions.CultureInvariant)]
    private static partial Regex PasswordRegex();
}
