using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Repository.Layer
{
    public partial class GenericRepository<TEntity, TKey, TContext>
    {
        public async Task<Guid> Create(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to create a null entity: {EntityType}", typeof(TEntity).Name);
                return Guid.Empty;
            }

            try
            {
                await _context.Set<TEntity>().AddAsync(entity);

                var idProperty = entity.GetType().GetProperty("Id");
                if (idProperty == null || idProperty.PropertyType != typeof(Guid))
                {
                    _logger.LogError("Entity {EntityType} does not have a Guid Id property", typeof(TEntity).Name);
                    return Guid.Empty;
                }

                return (Guid)(idProperty.GetValue(entity) ?? Guid.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity: {EntityType}", typeof(TEntity).Name);
                return Guid.Empty;
            }
        }

        public Task<bool> Update(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to update a null entity: {EntityType}", typeof(TEntity).Name);
                return Task.FromResult(false);
            }

            try
            {
                _context.Set<TEntity>().Update(entity);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity: {EntityType}", typeof(TEntity).Name);
                return Task.FromResult(false);
            }
        }

        public async Task BulkInsertAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
            {
                return;
            }

            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public Task<bool> Delete(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to delete a null entity: {EntityType}", typeof(TEntity).Name);
                return Task.FromResult(false);
            }

            try
            {
                _context.Set<TEntity>().Remove(entity);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity: {EntityType}", typeof(TEntity).Name);
                return Task.FromResult(false);
            }
        }

        public async Task<bool> SoftDeleteAsync(TKey id)
        {
            var entity = await Get(id);
            if (entity == null)
            {
                return false;
            }

            var prop = typeof(TEntity).GetProperty("IsDeleted");
            if (prop == null || prop.PropertyType != typeof(bool))
            {
                return false;
            }

            prop.SetValue(entity, true);
            _context.Set<TEntity>().Update(entity);
            return true;
        }

        public Task<int> DeleteWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate).ExecuteDeleteAsync();
        }

        public Task<int> UpdateWhereAsync(
            Expression<Func<TEntity, bool>> predicate,
            Action<UpdateSettersBuilder<TEntity>> setProps)
        {
            return _context.Set<TEntity>().Where(predicate).ExecuteUpdateAsync(setProps);
        }

        public Task<int> SoftDeleteWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>()
                .Where(predicate)
                .ExecuteUpdateAsync(s => s.SetProperty(e => EF.Property<bool>(e, "IsDeleted"), _ => true));
        }
    }
}
