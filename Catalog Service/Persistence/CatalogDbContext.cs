using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Occasion> Occasions => Set<Occasion>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductOccasion> ProductOccasions => Set<ProductOccasion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Occasion>(builder =>
        {
            builder.ToTable("Occasions");
            builder.HasKey(occasion => occasion.Id);
            builder.Property(occasion => occasion.Name).HasMaxLength(150).IsRequired();
            builder.Property(occasion => occasion.ImageUrl).HasMaxLength(2048).IsRequired();
            builder.HasIndex(occasion => new { occasion.IsArchived, occasion.DisplayOrder });
        });

        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(product => product.Id);
            builder.Property(product => product.Name).HasMaxLength(250).IsRequired();
            builder.Property(product => product.ImageUrl).HasMaxLength(2048).IsRequired();
            builder.Property(product => product.Price).HasPrecision(18, 2);
            builder.HasIndex(product => product.IsArchived);
        });

        modelBuilder.Entity<ProductOccasion>(builder =>
        {
            builder.ToTable("ProductOccasions");
            builder.HasKey(productOccasion => new { productOccasion.ProductId, productOccasion.OccasionId });
            builder.HasOne(productOccasion => productOccasion.Product)
                .WithMany(product => product.ProductOccasions)
                .HasForeignKey(productOccasion => productOccasion.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(productOccasion => productOccasion.Occasion)
                .WithMany(occasion => occasion.ProductOccasions)
                .HasForeignKey(productOccasion => productOccasion.OccasionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
