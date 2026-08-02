using System.Data;

namespace Base.Repository
{
    public partial interface IGenericRepository<TEntity, TKey>
        where TEntity : class
    {
        Task ExecuteInTransactionAsync(Func<Task> action, IsolationLevel? isolation = null);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
