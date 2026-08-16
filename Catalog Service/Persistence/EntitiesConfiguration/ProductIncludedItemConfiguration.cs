using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class ProductIncludedItemConfiguration : IEntityTypeConfiguration<ProductIncludedItem>
{
    public void Configure(EntityTypeBuilder<ProductIncludedItem> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.HasIndex(item => new { item.ProductId, item.SortOrder })
            .IsUnique();

        builder.HasOne(item => item.Product)
            .WithMany(product => product.IncludedItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
