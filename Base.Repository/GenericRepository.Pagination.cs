using Base.Repository.Utilities;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Repository.Layer
{
    public partial class GenericRepository<TEntity, TKey, TContext>
    {
        public Task<PaginatedResult<TEntity>> GetAllPaginatedAsync(int pageIndex, int pageSize)
        {
            return ToPaginatedResultAsync(_context.Set<TEntity>(), pageIndex, pageSize);
        }

        public Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(int pageIndex, int pageSize)
        {
            return ToPaginatedResultAsync(_context.Set<TEntity>().AsNoTracking(), pageIndex, pageSize);
        }

        public Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, bool>>[] predicates)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

            if (predicates != null && predicates.Length > 0)
            {
                foreach (var predicate in predicates)
                {
                    query = ApplyFilter(query, predicate);
                }
            }

            return ToPaginatedResultAsync(query, pageIndex, pageSize);
        }

        public Task<PaginatedResult<TEntity>> GetAllWithSpecsPaginatedAsync(
            ISpecification<TEntity> spec,
            int pageIndex,
            int pageSize)
        {
            return ToPaginatedResultAsync(ApplySpecification(spec), pageIndex, pageSize);
        }

        public Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return ToPaginatedResultAsync(CreateQuery(includes: includes), pageIndex, pageSize);
        }

        public Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsNoTrackingAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return ToPaginatedResultAsync(CreateQuery(true, includes), pageIndex, pageSize);
        }

        public async Task<PaginatedResult<TOut>> GetPageSelectAsync<TOut>(
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TOut>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
            where TOut : class
        {
            var query = CreateQuery(asNoTracking, includes);
            query = ApplyFilter(query, predicate);
            query = ApplyOrdering(query, orderBy);

            var normalized = NormalizePagination(pageIndex, pageSize);
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(GetSkipCount(normalized.PageIndex, normalized.PageSize))
                .Take(normalized.PageSize)
                .Select(selector)
                .ToListAsync();

            return new PaginatedResult<TOut>(
                totalCount,
                normalized.PageIndex,
                normalized.PageSize,
                items);
        }

        public Task<List<TEntity>> GetSliceAsync<TOrderKey>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TOrderKey>> orderBy,
            TOrderKey afterKey,
            int take,
            bool ascending = true,
            bool asNoTracking = true)
        {
            var query = CreateQuery(asNoTracking);
            query = ApplyFilter(query, predicate);

            if (ascending)
            {
                query = query
                    .Where(e => Comparer<TOrderKey>.Default.Compare(orderBy.Compile()(e), afterKey) > 0)
                    .OrderBy(orderBy);
            }
            else
            {
                query = query
                    .Where(e => Comparer<TOrderKey>.Default.Compare(orderBy.Compile()(e), afterKey) < 0)
                    .OrderByDescending(orderBy);
            }

            return query.Take(take).ToListAsync();
        }
    }
}
