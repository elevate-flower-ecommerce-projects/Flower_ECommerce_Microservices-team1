namespace Identity_service.Features.Users.UpdateProfile;

public interface IUpdateProfileValidator
{
    Task<Dictionary<string, string[]>> ValidateAsync(
        UpdateProfileCommand request,
        CancellationToken cancellationToken);
}
