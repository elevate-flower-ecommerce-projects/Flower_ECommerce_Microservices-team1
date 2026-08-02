using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Repository.Layer
{
    public partial class GenericRepository<TEntity, TKey, TContext>
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action, IsolationLevel? isolation = null)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = isolation.HasValue
                    ? await _context.Database.BeginTransactionAsync(isolation.Value)
                    : await _context.Database.BeginTransactionAsync();

                await action();
                await transaction.CommitAsync();
            });
        }
    }
}
