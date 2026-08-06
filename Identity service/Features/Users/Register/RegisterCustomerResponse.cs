namespace Identity_service.Features.Users.Register;

public sealed record RegisterCustomerResponse(
    string UserId,
    string Email,
    string Role,
    string Status);
