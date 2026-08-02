using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Base.Repository
{
    public partial interface IGenericRepository<TEntity, TKey>
        where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> spec);
        Task<List<TEntity>> GetAllAsNoTracking();
        Task<List<TEntity>> GetAllAsNoTracking(ISpecification<TEntity> spec);
        Task<List<TEntity>> GetAllAsNoTracking(Expression<Func<TEntity, bool>> spec);
        Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> spec);
        Task<List<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetAllDynamicAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> GetWithSpecs(ISpecification<TEntity> spec);
        Task<TEntity?> Get(TKey id);
        Task<TEntity?> Get(Expression<Func<TEntity, bool>> spec);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        Task<List<TEntity>> ListAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        Task<List<TOut>> ListSelectAsync<TOut>(
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TOut>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> ExistsByIdAsync(TKey id);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountSpecsAsync(ISpecification<TEntity> spec);
        Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, bool asNoTracking = true);

        IQueryable<TEntity> Query(bool asNoTracking = true, params Expression<Func<TEntity, object>>[] includes);
    }
}
