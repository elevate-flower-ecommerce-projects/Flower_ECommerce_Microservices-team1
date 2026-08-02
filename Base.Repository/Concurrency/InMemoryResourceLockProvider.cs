using System.Collections.Concurrent;

namespace Base.Repository.Concurrency;

public sealed class InMemoryResourceLockProvider : IResourceLockProvider
{
    public static InMemoryResourceLockProvider Shared { get; } = new();

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string resourceKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);
        cancellationToken.ThrowIfCancellationRequested();

        var semaphore = _locks.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));
        var acquired = await semaphore.WaitAsync(0, cancellationToken);

        return acquired
            ? new InMemoryResourceLock(resourceKey, semaphore, _locks)
            : null;
    }

    private sealed class InMemoryResourceLock : IAsyncDisposable
    {
        private readonly string _resourceKey;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;

        public InMemoryResourceLock(
            string resourceKey,
            SemaphoreSlim semaphore,
            ConcurrentDictionary<string, SemaphoreSlim> locks)
        {
            _resourceKey = resourceKey;
            _semaphore = semaphore;
            _locks = locks;
        }

        public ValueTask DisposeAsync()
        {
            _semaphore.Release();

            if (_semaphore.CurrentCount == 1)
            {
                _locks.TryRemove(new KeyValuePair<string, SemaphoreSlim>(_resourceKey, _semaphore));
            }

            return ValueTask.CompletedTask;
        }
    }
}
