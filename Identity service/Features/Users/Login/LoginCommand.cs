namespace Identity_service.Features.Users.Login;

using Identity_service.Abstractions;
using MediatR;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<Result<LoginResponseDto>>;
