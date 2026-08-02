using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Base.Repository
{
    public partial interface IGenericRepository<TEntity, TKey>
        where TEntity : class
    {
        Task<Guid> Create(TEntity entity);
        Task BulkInsertAsync(IEnumerable<TEntity> entities);
        Task<bool> Update(TEntity entity);
        Task<bool> Delete(TEntity entity);
        Task<bool> SoftDeleteAsync(TKey id);

        Task<int> DeleteWhereAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> UpdateWhereAsync(
            Expression<Func<TEntity, bool>> predicate,
            Action<UpdateSettersBuilder<TEntity>> setProps);
        Task<int> SoftDeleteWhereAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
