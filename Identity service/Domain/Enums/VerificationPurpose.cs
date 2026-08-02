namespace Identity_service.Domain.Enums;

public enum VerificationPurpose
{
    EmailConfirmation = 1,
    PhoneConfirmation = 2,
    PasswordReset = 3,
    TwoFactor = 4
}
