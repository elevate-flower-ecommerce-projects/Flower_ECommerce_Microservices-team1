using Base.Repository;
using Base.Repository.Concurrency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Layer.Interfaces;

namespace Repository.Layer;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositoryLayer<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddSingleton<IResourceLockProvider>(InMemoryResourceLockProvider.Shared);
        services.AddScoped(typeof(IUnitOfWork<TContext>), typeof(UnitOfWork<TContext>));

        return services;
    }
}
