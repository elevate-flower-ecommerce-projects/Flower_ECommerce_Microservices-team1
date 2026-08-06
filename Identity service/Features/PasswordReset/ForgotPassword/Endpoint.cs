using System.ComponentModel.DataAnnotations;
using Carter;
using Flower.Common.StandardizedResponse;
using Identity_service.Services;

namespace Identity_service.Features.PasswordReset.ForgotPassword;

public sealed class ForgotPasswordEndpoint : ICarterModule
{
    private const string Message = "If this email is registered, a code has been sent.";

    public void AddRoutes(IEndpointRouteBuilder app)
        => app.MapPost("/auth/forgot-password", HandleAsync).WithTags("Authentication").AllowAnonymous();

    private static async Task<IResult> HandleAsync(ForgotPasswordRequest request, PasswordResetOtpService otpService, PasswordResetEmailService emailService, ILogger<ForgotPasswordEndpoint> logger, CancellationToken cancellationToken)
    {
        if (!new EmailAddressAttribute().IsValid(request.Email))
            return OperationResultFactory.BadRequest("A valid email is required.", "A valid email is required.").ToHttpResult();

        var email = request.Email.Trim().ToLowerInvariant();
        var issue = await otpService.IssueAsync(email, cancellationToken);

        if (issue is not null && issue.CooldownRemainingSeconds == 0)
        {
            try
            {
                await emailService.SendAsync(email, issue.Otp, issue.ExpiresAtUtc, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Password reset email provider failed for request {PasswordResetRequestId}", issue.RequestId);
                await otpService.InvalidateAsync(issue.RequestId, cancellationToken);
            }
        }

        return OperationResultFactory.Success(new ForgotPasswordResponse(30), Message, Message, (StatusCode)StatusCodes.Status202Accepted).ToHttpResult();
    }
}
