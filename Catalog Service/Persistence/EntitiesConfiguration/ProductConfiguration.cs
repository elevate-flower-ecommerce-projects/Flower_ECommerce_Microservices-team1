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

        builder.Property(product => product.Price)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(product => product.StoreId);
        builder.HasIndex(product => product.SoldCount);
    }
}
