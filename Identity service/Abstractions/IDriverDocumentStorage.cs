namespace Identity_service.Abstractions;

public sealed record StoredDriverDocument(
    string StorageKey,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes);

public interface IDriverDocumentStorage
{
    Task<StoredDriverDocument> SaveAsync(
        Guid applicationId,
        IFormFile file,
        CancellationToken cancellationToken);
}
