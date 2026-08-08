using Identity_service.Contracts.Admins;

namespace Identity_service.Features.Admins.Login;

public record AdminLoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent) : IRequest<Result<LoginResponse>>;

public class AdminLoginCommandValidator : AbstractValidator<AdminLoginCommand>
{
    public AdminLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
