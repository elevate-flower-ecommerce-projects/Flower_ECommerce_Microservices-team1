namespace Identity_service.Features.PasswordReset.ResetPassword;

public sealed record ResetPasswordRequest(string ResetToken, string NewPassword, string ConfirmPassword);
