namespace Identity_service.Features.Users.UpdateProfile;

internal static class UpdateProfileMessages
{
    public const string ProfileLoaded = "Profile loaded successfully.";
    public const string ProfileUpdated = "Profile updated successfully.";
    public const string EmailChangedSignOut = "Profile updated. Please sign in again with your new email.";

    public const string UserNotFound = "User was not found.";
    public const string MissingIdentity = "Missing user identity.";
    public const string ValidationFailed = "Profile validation failed.";
    public const string UpdateFailed = "Unable to update the profile at this time.";

    // Worded exactly like registration so the app can show one message for the same problem.
    public const string EmailAlreadyRegistered = "Email already registered";
    public const string PhoneNumberAlreadyRegistered = "Phone number already registered";
}
