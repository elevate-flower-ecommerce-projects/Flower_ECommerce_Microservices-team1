using Base.Repository;
using Base.Repository.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Repository.Layer
{
    public partial class GenericRepository<TEntity, TKey, TContext> : IGenericRepository<TEntity, TKey>
        where TEntity : class
        where TContext : DbContext
    {
        protected readonly TContext _context;
        private readonly ILogger<GenericRepository<TEntity, TKey, TContext>> _logger;

        public GenericRepository(TContext context, ILogger<GenericRepository<TEntity, TKey, TContext>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private IQueryable<TEntity> CreateQuery(
            bool asNoTracking = false,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return ApplyIncludes(query, includes);
        }

        private static IQueryable<TEntity> ApplyIncludes(
            IQueryable<TEntity> query,
            params Expression<Func<TEntity, object>>[] includes)
        {
            if (includes == null || includes.Length == 0)
            {
                return query;
            }

            foreach (var include in includes)
            {
                if (include != null)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }

        private static IQueryable<TEntity> ApplyFilter(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, bool>>? predicate)
        {
            return predicate == null ? query : query.Where(predicate);
        }

        private static IQueryable<TEntity> ApplyOrdering(
            IQueryable<TEntity> query,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy)
        {
            return orderBy == null ? query : orderBy(query);
        }

        private static (int PageIndex, int PageSize) NormalizePagination(int pageIndex, int pageSize)
        {
            return (pageIndex <= 0 ? 1 : pageIndex, pageSize <= 0 ? 10 : pageSize);
        }

        private static int GetSkipCount(int pageIndex, int pageSize)
        {
            return (pageIndex - 1) * pageSize;
        }

        private static async Task<PaginatedResult<TItem>> ToPaginatedResultAsync<TItem>(
            IQueryable<TItem> query,
            int pageIndex,
            int pageSize)
            where TItem : class
        {
            var normalized = NormalizePagination(pageIndex, pageSize);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(GetSkipCount(normalized.PageIndex, normalized.PageSize))
                .Take(normalized.PageSize)
                .ToListAsync();

            return new PaginatedResult<TItem>(
                totalCount,
                normalized.PageIndex,
                normalized.PageSize,
                items);
        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            return SpecificationEvaluator<TEntity>
                .GetQuery(_context.Set<TEntity>().AsQueryable(), specification);
        }

        private IQueryable<TEntity> ApplySpecificationAsNoTracking(ISpecification<TEntity> specification)
        {
            return SpecificationEvaluator<TEntity>
                .GetQuery(_context.Set<TEntity>().AsNoTracking(), specification);
        }
    }
}
