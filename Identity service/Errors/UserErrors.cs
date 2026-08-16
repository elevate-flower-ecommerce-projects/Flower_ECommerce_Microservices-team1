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

    public static readonly Error AccountLocked =
        new("User.AccountLocked", "Too many failed login attempts. Your account is temporarily locked.", StatusCodes.Status423Locked);

    public static readonly Error AccountDisabled =
        new("User.AccountDisabled", "This account is disabled.", StatusCodes.Status403Forbidden);

    public static readonly Error UserNotAdmin =
        new("User.NotAdmin", "The provided user is not an administrator.", StatusCodes.Status403Forbidden);

    public static readonly Error EmailIsNotConfirmed =
        new("User.EmailIsNotConfirmed", "The provided email is not confirmed.", StatusCodes.Status400BadRequest);

    public static readonly Error LockedUser =
        new("User.LockedUser", "The provided user is locked.", StatusCodes.Status403Forbidden);

    public static readonly Error InvalidToken =
        new("User.InvalidToken", "The provided token is invalid.", StatusCodes.Status401Unauthorized);
}
