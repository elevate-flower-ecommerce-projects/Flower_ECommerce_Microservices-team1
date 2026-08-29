using Address___Store_Coverage_Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Address___Store_Coverage_Service.Persistence;

public sealed class AddressDbContext(DbContextOptions<AddressDbContext> options) : DbContext(options)
{
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreCoverageArea> StoreCoverageAreas => Set<StoreCoverageArea>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<City> Cities => Set<City>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AddressDbContext).Assembly);
    }
}
