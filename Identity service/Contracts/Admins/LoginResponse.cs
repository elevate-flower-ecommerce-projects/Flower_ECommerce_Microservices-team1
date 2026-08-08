namespace Identity_service.Contracts.Admins;

public record LoginResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);