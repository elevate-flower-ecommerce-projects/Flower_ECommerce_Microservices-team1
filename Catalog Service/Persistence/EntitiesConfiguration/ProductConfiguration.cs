using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(product => product.ImageUrl)
            .HasMaxLength(512);

        builder.Property(product => product.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(product => product.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(product => product.DiscountStartsAtUtc)
            .IsRequired(false);

        builder.Property(product => product.DiscountEndsAtUtc)
            .IsRequired(false);

        builder.Property(product => product.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(product => product.SoldCount);
        builder.HasIndex(product => product.CreatedAtUtc);
    }
}
