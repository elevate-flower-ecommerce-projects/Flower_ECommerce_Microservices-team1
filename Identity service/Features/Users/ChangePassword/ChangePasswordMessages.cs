namespace Identity_service.Features.Users.ChangePassword;

public static class ChangePasswordMessages
{
    public const string MissingIdentity = "Missing user identity.";
    public const string UserNotFound = "User not found.";
    public const string ValidationFailed = "Password change validation failed.";
    public const string CurrentPasswordIncorrect = "Current password is incorrect.";
    public const string NewPasswordSameAsCurrent = "New password must be different from the current password.";
    public const string PasswordChanged = "Your password was changed. Please sign in again.";
}
