namespace Identity_service.Features.Users.UpdateProfile;

public static class UserProfileMapping
{
    public static UserProfileResponse ToProfileResponse(
        this ApplicationUser user,
        IEnumerable<string> roles,
        bool emailChanged = false)
        => new(
            user.Id,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email,
            user.PhoneNumber,
            user.Gender,
            user.ProfilePictureUrl,
            [.. roles],
            emailChanged);
}
