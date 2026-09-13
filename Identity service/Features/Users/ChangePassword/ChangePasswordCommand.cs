using Flower.Common.StandardizedResponse;

namespace Identity_service.Features.Users.ChangePassword;

public sealed record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : IRequest<OperationResult<object>>;
