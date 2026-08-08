using Identity_service.Contracts.Admins;

namespace Identity_service.Features.Users.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

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