namespace Identity_service.Abstractions;

public interface IAvatarStorage
{
    /// <summary>
    /// Stores the avatar under wwwroot and returns the relative URL clients can request directly,
    /// for example /uploads/avatars/{guid}.jpg.
    /// </summary>
    Task<string> SaveAsync(string userId, IFormFile file, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a previously stored avatar. Missing files are ignored so a replaced avatar never
    /// fails the request that already succeeded.
    /// </summary>
    Task DeleteAsync(string? relativeUrl, CancellationToken cancellationToken);
}
