namespace Identity_service.Features.PasswordReset.VerifyResetOtp;

public sealed record VerifyResetOtpResponse(string Status, string ResetToken, DateTime ExpiresAtUtc);
