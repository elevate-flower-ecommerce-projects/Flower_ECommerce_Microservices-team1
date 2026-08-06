using Carter;
using Flower.Common.StandardizedResponse;
using Identity_service.Entities;
using Identity_service.Services;
using Identity_service.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity_service.Features.PasswordReset.ResetPassword;

public sealed class ResetPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
        => app.MapPost("/auth/reset-password", HandleAsync).WithTags("Authentication").AllowAnonymous();

    private static async Task<IResult> HandleAsync(ResetPasswordRequest request, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return OperationResultFactory.BadRequest("Password confirmation does not match.", "Password confirmation does not match.").ToHttpResult();

        var now = DateTime.UtcNow;
        var resetRequest = await dbContext.PasswordResetRequests
            .Include(candidate => candidate.User)
            .Where(candidate => candidate.ResetTokenHash == ResetTokenHash.Create(request.ResetToken) && candidate.ResetTokenExpiresAtUtc > now && candidate.ConsumedAtUtc == null && candidate.InvalidatedAtUtc == null)
            .OrderByDescending(candidate => candidate.VerifiedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetRequest?.User is null)
            return OperationResultFactory.UnAuthorized("The password reset authorization is invalid or expired.", "The password reset authorization is invalid or expired.").ToHttpResult();


        var reset = await userManager.ResetPasswordAsync(resetRequest.User, request.ResetToken, request.NewPassword);
        if (!reset.Succeeded)
            return OperationResultFactory.BadRequest(reset.Errors.Select(error => error.Description).ToArray(), "The provided password does not meet the password policy.", "The provided password does not meet the password policy.").ToHttpResult();


        foreach (var token in await dbContext.RefreshTokens.Where(token => token.UserId == resetRequest.UserId && token.RevokedOn == null).ToListAsync(cancellationToken))
            token.RevokedOn = now;

        await userManager.UpdateSecurityStampAsync(resetRequest.User);
        resetRequest.ConsumedAtUtc = now;

        var auditEvent = new PasswordResetAuditEvent
        {
            ResetRequestId = resetRequest.Id,
            UserId = resetRequest.UserId,
            OccurredAtUtc = now,
            EventType = "PasswordChanged"
        };

        dbContext.PasswordResetAuditEvents.Add(auditEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResultFactory.Success(message: ResetPasswordResponse.SuccessMessage, messageLocalized: ResetPasswordResponse.SuccessMessage).ToHttpResult();
    }
}
