using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Identity_service.Entities;
using Identity_service.Persistence;
using Identity_service.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity_service.Services;

public sealed class PasswordResetOtpService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IOptions<PasswordResetOptions> options)
{
    private readonly PasswordResetOptions _options = Validate(options.Value);

    internal async Task<PasswordResetOtpIssue?> IssueAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user?.Email is null) return null;

        var now = DateTime.UtcNow;
        var requests = await dbContext.PasswordResetRequests
            .Where(request => request.UserId == user.Id && request.ConsumedAtUtc == null && request.InvalidatedAtUtc == null)
            .OrderByDescending(request => request.LastSentAtUtc).ToListAsync(cancellationToken);

        var latest = requests.FirstOrDefault();
        if (latest is not null && now < latest.LastSentAtUtc.AddSeconds(_options.ResendCooldownSeconds))
            return new(latest.Id, string.Empty, latest.ExpiresAtUtc, (int)Math.Ceiling((latest.LastSentAtUtc.AddSeconds(_options.ResendCooldownSeconds) - now).TotalSeconds));

        foreach (var request in requests)
            request.InvalidatedAtUtc = now;

        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        var reset = new PasswordResetRequest
        {
            UserId = user.Id,
            OtpHash = HashOtp(otp),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(_options.OtpLifetimeMinutes),
            LastSentAtUtc = now,
            AttemptsRemaining = _options.MaxAttempts
        };

        dbContext.PasswordResetRequests.Add(reset);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new(reset.Id, otp, reset.ExpiresAtUtc, 0);
    }

    internal async Task<PasswordResetOtpVerificationResult> VerifyAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null || otp.Length != 6 || !otp.All(char.IsAsciiDigit)) return PasswordResetOtpVerificationResult.Invalid;

        var request = await dbContext.PasswordResetRequests
            .Where(candidate => candidate.UserId == user.Id && candidate.InvalidatedAtUtc == null && candidate.VerifiedAtUtc == null && candidate.ConsumedAtUtc == null)
            .OrderByDescending(candidate => candidate.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (request is null) return PasswordResetOtpVerificationResult.Invalid;

        var now = DateTime.UtcNow;
        if (now >= request.ExpiresAtUtc) return PasswordResetOtpVerificationResult.Expired;
        if (CryptographicOperations.FixedTimeEquals(Convert.FromHexString(request.OtpHash), Convert.FromHexString(HashOtp(otp))))
        {
            request.VerifiedAtUtc = now;
            await dbContext.SaveChangesAsync(cancellationToken);
            return PasswordResetOtpVerificationResult.Valid;
        }

        if (--request.AttemptsRemaining <= 0)
        {
            request.AttemptsRemaining = 0;
            request.InvalidatedAtUtc = now;
            await dbContext.SaveChangesAsync(cancellationToken);
            return PasswordResetOtpVerificationResult.AttemptsExceeded;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return PasswordResetOtpVerificationResult.Invalid;
    }

    public async Task InvalidateAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.PasswordResetRequests.FindAsync([requestId], cancellationToken);
        if (request is null || request.InvalidatedAtUtc is not null) return;
        request.InvalidatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }


    #region Helper Methods

    private string HashOtp(string otp)
        => Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(_options.OtpHashKey), Encoding.UTF8.GetBytes(otp)));

    private static PasswordResetOptions Validate(PasswordResetOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.OtpHashKey) || options.OtpLifetimeMinutes <= 0 || options.MaxAttempts <= 0 || options.ResendCooldownSeconds < 0)
            throw new InvalidOperationException("Password reset OTP configuration is invalid.");
        return options;
    }

    #endregion

}

internal sealed record PasswordResetOtpIssue(Guid RequestId, string Otp, DateTime ExpiresAtUtc, int CooldownRemainingSeconds);
internal enum PasswordResetOtpVerificationResult
{
    Valid,
    Invalid,
    Expired,
    AttemptsExceeded
}
