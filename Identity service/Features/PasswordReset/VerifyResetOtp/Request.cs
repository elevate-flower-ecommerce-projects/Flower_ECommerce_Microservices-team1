namespace Identity_service.Features.PasswordReset.VerifyResetOtp;

public sealed record VerifyResetOtpRequest(string Email, string Otp);
