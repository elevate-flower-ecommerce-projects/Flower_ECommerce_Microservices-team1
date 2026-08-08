namespace Identity_service.Contracts.Users;

public record UserResponse(
    string Id,
    string UserName,
    string Email,
    string PhoneNumber
);
//tst