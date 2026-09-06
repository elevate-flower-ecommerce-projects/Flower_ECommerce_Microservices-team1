using Identity_service.Abstractions;
using Identity_service.Settings;
using Microsoft.Extensions.Options;

namespace Identity_service.Services;

/// <summary>
/// Stores avatars inside wwwroot so they can be served as ordinary static files.
/// Driver documents deliberately live outside wwwroot because only an admin may read them; an
/// avatar is shown on every screen, so a plain URL is the right trade-off here.
/// </summary>
public sealed class LocalAvatarStorage(
    IOptions<AvatarStorageOptions> options,
    IWebHostEnvironment environment,
    ILogger<LocalAvatarStorage> logger) : IAvatarStorage
{
    public async Task<string> SaveAsync(string userId, IFormFile file, CancellationToken cancellationToken)
    {
        var relativePath = options.Value.RelativePath.Trim('/', '\\');

        // A random file name keeps avatars unguessable even though the folder is public.
        var extension = ResolveExtension(file);
        var fileName = $"{Guid.CreateVersion7():N}{extension}";

        var directory = Path.Combine(WebRootPath, relativePath);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, fileName);
        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        logger.LogInformation("Stored a new avatar for user {UserId}.", userId);

        return $"/{relativePath.Replace('\\', '/')}/{fileName}";
    }

    public Task DeleteAsync(string? relativeUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return Task.CompletedTask;

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(WebRootPath, relativePath));
        var webRoot = Path.GetFullPath(WebRootPath);

        // Guard against a stored value escaping wwwroot, matching LocalDriverDocumentStorage.
        if (!fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Refused to delete an avatar outside the web root: {Path}", relativeUrl);
            return Task.CompletedTask;
        }

        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (IOException exception)
        {
            // The new avatar is already saved, so a failed cleanup must not fail the request.
            logger.LogWarning(exception, "Could not delete the previous avatar {Path}.", relativeUrl);
        }

        return Task.CompletedTask;
    }

    /// <summary>WebRootPath is null when wwwroot does not exist yet on a fresh checkout.</summary>
    private string WebRootPath =>
        environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");

    private static string ResolveExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Content type has already been validated, so fall back to it when the name carries no
        // usable extension (common for camera captures on mobile).
        return extension is ".jpg" or ".jpeg" or ".png"
            ? extension
            : file.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ? ".png" : ".jpg";
    }
}
