using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Repository.Layer
{
    public partial class GenericRepository<TEntity, TKey, TContext>
    {
        public async Task<TEntity?> Get(TKey id)
        {
            try
            {
                return await _context.Set<TEntity>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity by ID: {EntityType}, ID: {EntityId}", typeof(TEntity).Name, id);
                return null;
            }
        }

        public async Task<TEntity?> Get(Expression<Func<TEntity, bool>> spec)
        {
            try
            {
                return await _context.Set<TEntity>().FirstOrDefaultAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity by specification: {EntityType}", typeof(TEntity).Name);
                return null;
            }
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>();
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> spec)
        {
            try
            {
                return await _context.Set<TEntity>().Where(spec).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities by condition: {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<List<TEntity>> GetAllAsNoTracking()
        {
            try
            {
                return await _context.Set<TEntity>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all entities (No Tracking): {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<List<TEntity>> GetAllAsNoTracking(Expression<Func<TEntity, bool>> spec)
        {
            try
            {
                return await _context.Set<TEntity>().AsNoTracking().Where(spec).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities (No Tracking) by condition: {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<List<TEntity>> GetAllAsNoTracking(ISpecification<TEntity> spec)
        {
            return await ApplySpecificationAsNoTracking(spec).ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> spec)
        {
            try
            {
                return await ApplySpecification(spec).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities with specifications: {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<TEntity?> GetWithSpecs(ISpecification<TEntity> spec)
        {
            try
            {
                return await ApplySpecification(spec).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity with specifications: {EntityType}", typeof(TEntity).Name);
                return null;
            }
        }

        public Task<List<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            return CreateQuery(includes: includes).ToListAsync();
        }

        public Task<TEntity?> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            return CreateQuery(includes: includes)
                .FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id")!.Equals(id));
        }

        public Task<List<TEntity>> GetAllDynamicAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = CreateQuery(includes: includes);
            query = ApplyFilter(query, filter);
            query = ApplyOrdering(query, orderBy);

            return query.ToListAsync();
        }

        public Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return CreateQuery(asNoTracking, includes).FirstOrDefaultAsync(predicate);
        }

        public Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return CreateQuery(asNoTracking, includes).SingleOrDefaultAsync(predicate);
        }

        public Task<List<TEntity>> ListAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = CreateQuery(asNoTracking, includes);
            query = ApplyFilter(query, predicate);
            query = ApplyOrdering(query, orderBy);

            return query.ToListAsync();
        }

        public Task<List<TOut>> ListSelectAsync<TOut>(
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TOut>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = CreateQuery(asNoTracking, includes);
            query = ApplyFilter(query, predicate);
            query = ApplyOrdering(query, orderBy);

            return query.Select(selector).ToListAsync();
        }

        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().AnyAsync(predicate);
        }

        public Task<bool> ExistsByIdAsync(TKey id)
        {
            return _context.Set<TEntity>().AnyAsync(e => EF.Property<TKey>(e, "Id")!.Equals(id));
        }

        public Task<int> CountAsync()
        {
            return _context.Set<TEntity>().CountAsync();
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().CountAsync(predicate);
        }

        public Task<int> CountSpecsAsync(ISpecification<TEntity> spec)
        {
            return ApplySpecification(spec).CountAsync();
        }

        public async Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, bool asNoTracking = true)
        {
            if (ids == null)
            {
                return new List<TEntity>();
            }

            return await CreateQuery(asNoTracking)
                .Where(e => ids.Contains(EF.Property<TKey>(e, "Id")))
                .ToListAsync();
        }

        public IQueryable<TEntity> Query(
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return CreateQuery(asNoTracking, includes);
        }
    }
}
