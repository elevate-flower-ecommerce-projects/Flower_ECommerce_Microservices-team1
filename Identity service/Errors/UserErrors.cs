using Identity_service.Abstractions;

namespace Identity_service.Errors;

public static class UserErrors
{
    public static readonly Error UserNotFound =
        new("User.NotFound", "User not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Invalid email or password.", StatusCodes.Status401Unauthorized);
    public static readonly Error UserAlreadyExists =
        new("User.AlreadyExists", "User with the given email already exists.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidEmailFormat =
        new("User.InvalidEmailFormat", "The provided email format is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error PasswordTooWeak =
        new("User.PasswordTooWeak", "The provided password does not meet the strength requirements.", StatusCodes.Status400BadRequest);
}
