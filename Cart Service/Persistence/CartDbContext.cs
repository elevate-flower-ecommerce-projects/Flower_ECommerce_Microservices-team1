using Microsoft.EntityFrameworkCore;

namespace Cart_Service.Persistence;

public sealed class CartDbContext(DbContextOptions<CartDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Cart> Carts => Set<Entities.Cart>();
    public DbSet<Entities.CartItem> CartItems => Set<Entities.CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
    }
}
