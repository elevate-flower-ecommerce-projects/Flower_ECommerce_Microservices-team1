using Carter;
using Flower.Common.StandardizedResponse;
using Identity_service.Entities;
using Identity_service.Services;
using Identity_service.Persistence;
using Identity_service.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity_service.Features.PasswordReset.VerifyResetOtp;

public sealed class VerifyResetOtpEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
        => app.MapPost("/auth/verify-otp", HandleAsync).WithTags("Authentication").AllowAnonymous();

    private static async Task<IResult> HandleAsync(VerifyResetOtpRequest request, PasswordResetOtpService otpService, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IOptions<PasswordResetOptions> options, CancellationToken cancellationToken)
    {
        var verification = await otpService.VerifyAsync(request.Email.Trim().ToLowerInvariant(), request.Otp, cancellationToken);
        if (verification != PasswordResetOtpVerificationResult.Valid)
            return Failure(verification);

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        var resetRequest = await dbContext.PasswordResetRequests
            .Where(candidate => candidate.UserId == user!.Id && candidate.VerifiedAtUtc != null && candidate.ResetTokenHash == null && candidate.ConsumedAtUtc == null)
            .OrderByDescending(candidate => candidate.VerifiedAtUtc)
            .FirstAsync(cancellationToken);

        var token = await userManager.GeneratePasswordResetTokenAsync(user!);

        resetRequest.ResetTokenHash = ResetTokenHash.Create(token);
        resetRequest.ResetTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(options.Value.ResetTokenLifetimeMinutes);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResultFactory.Success(new VerifyResetOtpResponse("Valid", token, resetRequest.ResetTokenExpiresAtUtc.Value), "Reset code verified.", "Reset code verified.").ToHttpResult();
    }

    private static IResult Failure(PasswordResetOtpVerificationResult result) => result switch
    {
        PasswordResetOtpVerificationResult.Expired => OperationResultFactory.Error("Expired", "Expired", StatusCode.Gone).ToHttpResult(),
        PasswordResetOtpVerificationResult.AttemptsExceeded => OperationResultFactory.Error("AttemptsExceeded", "AttemptsExceeded", StatusCode.Locked).ToHttpResult(),
        _ => OperationResultFactory.BadRequest("Invalid", "Invalid").ToHttpResult()
    };
}
