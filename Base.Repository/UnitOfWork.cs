using Base.Repository;
using Base.Repository.Concurrency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repository.Layer.Interfaces;
using System.Collections;

namespace Repository.Layer
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IResourceLockProvider _resourceLockProvider;
        private readonly Hashtable _repositories;
        private readonly object _lock = new();

        public UnitOfWork(TContext context, IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _resourceLockProvider = serviceProvider.GetService<IResourceLockProvider>()
                ?? InMemoryResourceLockProvider.Shared;
            _repositories = new Hashtable();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public Task<IAsyncDisposable?> TryAcquireResourceLockAsync(
            string resourceKey,
            TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            return _resourceLockProvider.TryAcquireAsync(resourceKey, expiry, cancellationToken);
        }

        public IGenericRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            lock (_lock)
            {
                if (!_repositories.ContainsKey(entityType))
                {
                    var repoType = typeof(GenericRepository<,,>).MakeGenericType(entityType, typeof(TKey), typeof(TContext));

                    var loggerType = typeof(ILogger<>).MakeGenericType(repoType);
                    var logger = _serviceProvider.GetRequiredService(loggerType);

                    var repoInstance = Activator.CreateInstance(repoType, _context, logger);

                    if (repoInstance != null)
                    {
                        _repositories.Add(entityType, repoInstance);
                    }
                }
            }

            if (_repositories[entityType] is IGenericRepository<TEntity, TKey> repository)
            {
                return repository;
            }

            throw new InvalidOperationException($"Could not resolve repository for entity type {entityType.Name}.");
        }
    }
}
