namespace Identity_service.Contracts.Auth;

public record LoginResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Token,
    string RefreshToken

);