namespace Identity_service.Settings;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";
    public string OtpHashKey { get; set; } = string.Empty;
    public int OtpLifetimeMinutes { get; set; } = 10;
    public int MaxAttempts { get; set; } = 5;
    public int ResendCooldownSeconds { get; set; } = 30;
    public int ResetTokenLifetimeMinutes { get; set; } = 5;
}
