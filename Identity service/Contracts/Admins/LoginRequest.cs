namespace Identity_service.Contracts.Admins;

public record LoginRequest(
    string Email,
    string Password
);