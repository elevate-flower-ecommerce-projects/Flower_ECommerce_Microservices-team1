using System.Net;
using System.Net.Mail;
using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Settings;
using Microsoft.Extensions.Options;

namespace Identity_service.Services;

/// <summary>
/// Sends driver application decision notifications by email.
/// </summary>
public sealed class SmtpApplicantNotificationService(
    IOptions<EmailOptions> options,
    ILogger<SmtpApplicantNotificationService> logger) : IApplicantNotificationService
{
    public async Task NotifyDriverApplicationDecisionAsync(
        ApplicationUser applicant,
        DriverApplication application,
        CancellationToken cancellationToken)
    {
        #region Validate notification target

        if (string.IsNullOrWhiteSpace(applicant.Email))
        {
            logger.LogWarning(
                "Driver application decision notification skipped because applicant email is missing. UserId={UserId}, ApplicationId={ApplicationId}",
                applicant.Id,
                application.Id);
            return;
        }

        var emailOptions = options.Value;
        if (string.IsNullOrWhiteSpace(emailOptions.Host) || string.IsNullOrWhiteSpace(emailOptions.FromEmail))
        {
            logger.LogWarning(
                "Driver application decision notification skipped because SMTP settings are incomplete. ApplicationId={ApplicationId}",
                application.Id);
            return;
        }

        #endregion

        #region Build email

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.FromEmail, emailOptions.FromName),
            Subject = BuildSubject(application),
            Body = BuildBody(applicant, application),
            IsBodyHtml = true
        };
        message.To.Add(applicant.Email);

        using var client = new SmtpClient(emailOptions.Host, emailOptions.Port)
        {
            EnableSsl = emailOptions.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(emailOptions.UserName))
            client.Credentials = new NetworkCredential(emailOptions.UserName, emailOptions.Password);

        #endregion

        #region Send email

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Driver application decision email failed. UserId={UserId}, ApplicationId={ApplicationId}, Status={Status}",
                applicant.Id,
                application.Id,
                application.Status);
        }

        #endregion
    }

    #region Helpers

    private static string BuildSubject(DriverApplication application)
        => application.Status == DriverApplicationStatus.Approved
            ? "Your Flower driver application was approved"
            : "Your Flower driver application was reviewed";

    private static string BuildBody(ApplicationUser applicant, DriverApplication application)
    {
        var applicantName = $"{applicant.FirstName} {applicant.LastName}".Trim();
        var greetingName = string.IsNullOrWhiteSpace(applicantName) ? "there" : WebUtility.HtmlEncode(applicantName);

        if (application.Status == DriverApplicationStatus.Approved)
        {
            return $"""
                    <p>Hello {greetingName},</p>
                    <p>Your Flower driver application has been approved. You can now log in and receive delivery assignments.</p>
                    <p>Thank you,<br/>Flower Delivery Team</p>
                    """;
        }

        var reason = WebUtility.HtmlEncode(application.RejectionReason ?? "No reason was provided.");
        return $"""
                <p>Hello {greetingName},</p>
                <p>Your Flower driver application was rejected.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>Please contact support if you need help with this decision.</p>
                <p>Thank you,<br/>Flower Delivery Team</p>
                """;
    }

    #endregion
}
