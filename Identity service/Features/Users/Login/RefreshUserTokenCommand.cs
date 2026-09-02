using Identity_service.Abstractions;
using MediatR;

namespace Identity_service.Features.Users.Login;

public sealed record RefreshUserTokenCommand(string RefreshToken)
    : IRequest<Result<LoginResponseDto>>;

public sealed class RefreshUserTokenCommandValidator : AbstractValidator<RefreshUserTokenCommand>
{
    public RefreshUserTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
