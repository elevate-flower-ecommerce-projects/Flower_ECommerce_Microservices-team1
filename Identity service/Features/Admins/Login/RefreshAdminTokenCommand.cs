using Identity_service.Contracts.Admins;

namespace Identity_service.Features.Admins.Login;

public record RefreshAdminTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;

public sealed class RefreshAdminTokenCommandValidator : AbstractValidator<RefreshAdminTokenCommand>
{
    public RefreshAdminTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}
