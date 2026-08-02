using Base.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Layer.Interfaces
{
    public interface IUnitOfWork<TContext> where TContext : DbContext
    {
        IGenericRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class;
        Task<int> CompleteAsync();
        Task<IAsyncDisposable?> TryAcquireResourceLockAsync(
            string resourceKey,
            TimeSpan expiry,
            CancellationToken cancellationToken = default);
    }
}
