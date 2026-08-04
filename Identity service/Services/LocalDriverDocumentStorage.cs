using Identity_service.Abstractions;
using Identity_service.Settings;
using Microsoft.Extensions.Options;

namespace Identity_service.Services;

/// <summary>
/// Stores driver documents outside wwwroot and returns an internal storage key.
/// </summary>
public sealed class LocalDriverDocumentStorage(
    IOptions<DriverDocumentStorageOptions> options,
    IWebHostEnvironment environment) : IDriverDocumentStorage
{
    public async Task<StoredDriverDocument> SaveAsync(
        Guid applicationId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        #region Resolve private storage path

        var storageOptions = options.Value;
        var rootPath = Path.IsPathRooted(storageOptions.RootPath)
            ? storageOptions.RootPath
            : Path.Combine(environment.ContentRootPath, storageOptions.RootPath);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.CreateVersion7():N}{extension}";
        var relativeKey = Path.Combine("driver-applications", applicationId.ToString("N"), fileName);
        var fullPath = Path.Combine(rootPath, relativeKey);

        #endregion

        #region Persist document

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return new StoredDriverDocument(
            relativeKey.Replace('\\', '/'),
            Path.GetFileName(file.FileName),
            file.ContentType,
            file.Length);

        #endregion
    }
}
