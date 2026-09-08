using Identity_service.Abstractions;
using MediatR;

namespace Identity_service.Features.Users.Login;

public sealed record LoginCommand(string Email, string Password ,string FCMToken , string DeviceId)
    : IRequest<Result<LoginResponseDto>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
