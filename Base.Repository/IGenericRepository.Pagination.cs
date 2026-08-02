using Base.Repository.Utilities;
using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Base.Repository
{
    public partial interface IGenericRepository<TEntity, TKey>
        where TEntity : class
    {
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsync(int pageIndex, int pageSize);
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(int pageIndex, int pageSize);
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, bool>>[] predicates);
        Task<PaginatedResult<TEntity>> GetAllWithSpecsPaginatedAsync(
            ISpecification<TEntity> spec,
            int pageIndex,
            int pageSize);
        Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsNoTrackingAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, object>>[] includes);

        Task<PaginatedResult<TOut>> GetPageSelectAsync<TOut>(
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TOut>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
            where TOut : class;

        Task<List<TEntity>> GetSliceAsync<TOrderKey>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TOrderKey>> orderBy,
            TOrderKey afterKey,
            int take,
            bool ascending = true,
            bool asNoTracking = true);
    }
}
