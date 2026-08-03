namespace Identity_service.Contracts.Users;

public record UserResponse(
    Guid Id,
    string UserName,
    string Email,
    string PhoneNumber
);
