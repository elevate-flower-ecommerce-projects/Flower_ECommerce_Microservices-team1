namespace Identity_service.Features.Users.Login;

public sealed record RefreshUserTokenCommand(string RefreshToken, string? DeviceInfo)
    : IRequest<Result<LoginResponseDto>>;

public class RefreshUserTokenCommandValidator : AbstractValidator<RefreshUserTokenCommand>
{
    public RefreshUserTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
