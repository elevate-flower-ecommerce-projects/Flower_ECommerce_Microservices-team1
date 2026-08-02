namespace Base.Repository.Concurrency;

public interface IResourceLockProvider
{
    Task<IAsyncDisposable?> TryAcquireAsync(
        string resourceKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}
