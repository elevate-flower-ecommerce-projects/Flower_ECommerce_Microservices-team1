using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Identity_service.Features.Users;

/// <summary>
/// Field rules shared by the profile screens. These mirror
/// <see cref="Register.RegisterCustomerValidator"/> exactly, because AC3 requires editing a profile
/// to validate email and phone the same way registration does.
///
/// Registration still carries its own copy so this change does not touch that slice. Folding
/// RegisterCustomerValidator onto this class is a follow-up to coordinate with its author.
/// </summary>
public static partial class UserProfileFieldRules
{
    public const int MaxNamePartLength = 100;
    public const int MaxEmailLength = 256;

    public static bool IsValidEmail(string email)
        => email.Length <= MaxEmailLength && new EmailAddressAttribute().IsValid(email);

    public static bool IsValidEgyptianMobile(string phoneNumber)
        => EgyptianMobileRegex().IsMatch(phoneNumber);

    public static void AddIf(
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
}
