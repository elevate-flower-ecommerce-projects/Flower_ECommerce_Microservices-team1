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
        var host = string.IsNullOrWhiteSpace(_options.Host) ? "smtp.gmail.com" : _options.Host.Trim();
        var port = _options.Port > 0 ? _options.Port : 587;
        var username = (!string.IsNullOrWhiteSpace(_options.Username) ? _options.Username : _options.UserName).Trim();
        var password = _options.Password?.Trim().Replace(" ", "") ?? string.Empty;
        var fromAddress = !string.IsNullOrWhiteSpace(_options.FromAddress) 
            ? _options.FromAddress.Trim() 
            : (!string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.FromEmail.Trim() : username);
        var fromName = string.IsNullOrWhiteSpace(_options.FromName) ? "Flower Delivery" : _options.FromName.Trim();

        for (var retry = 0; ; retry++)
        {
            try
            {
                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = _options.UseStartTls,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000
                };

                using var message = new MailMessage(new MailAddress(fromAddress, fromName), new MailAddress(destinationEmail.Trim()))
                {
                    Subject = "Your Flower E-Commerce Password Reset Code",
                    Body = $"Your Flower E-Commerce password reset code is: {otp}\n\nThis code expires in 10 minutes ({expiresAtUtc:O}).\nIf you did not request this code, please ignore this email.",
                    IsBodyHtml = false
                };

                await client.SendMailAsync(message, cancellationToken);
                logger.LogInformation("Password reset email sent successfully to {DestinationEmail}", destinationEmail);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception) when (retry < _options.MaxRetries)
            {
                logger.LogWarning(exception, "Password reset email delivery failed to {DestinationEmail}. Retrying attempt {Attempt}.", destinationEmail, retry + 1);
                await Task.Delay(TimeSpan.FromSeconds(_options.InitialRetryDelaySeconds * Math.Pow(2, retry)), cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Password reset email delivery failed to {DestinationEmail} after {MaxRetries} retries.", destinationEmail, _options.MaxRetries);
                throw;
            }
        }
    }
}
