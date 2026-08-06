using System.Net;
using System.Net.Mail;
using Identity_service.Settings;
using Microsoft.Extensions.Options;

namespace Identity_service.Services;

public sealed class PasswordResetEmailService(IOptions<EmailOptions> options, ILogger<PasswordResetEmailService> logger)
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string destinationEmail, string otp, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port) { EnableSsl = _options.UseStartTls, Credentials = new NetworkCredential(_options.Username, _options.Password) };
            using var message = new MailMessage(new MailAddress(_options.FromAddress, _options.FromName), new MailAddress(destinationEmail))
            {
                Subject = "Your password reset code",
                Body = $"Your Flower E-Commerce password reset code is: {otp}.\n\nIt expires in 10 minutes ({expiresAtUtc:O}). Do not share this code with anyone.",
                IsBodyHtml = false
            };
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Password reset email delivery failed.");
            throw;
        }
    }
}
