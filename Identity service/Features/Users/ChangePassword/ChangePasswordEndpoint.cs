using Flower.Common.StandardizedResponse;
using Microsoft.AspNetCore.Mvc;

namespace Identity_service.Features.Users.ChangePassword;

public sealed class ChangePasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithTags("Authentication")
            .Produces<OperationResult<object>>()
            .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return OperationResultFactory.UnAuthorized(
                ChangePasswordMessages.MissingIdentity,
                ChangePasswordMessages.MissingIdentity)
                .ToHttpResult();
        }

        var result = await sender.Send(new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword), cancellationToken);

        return result.ToHttpResult();
    }
}
