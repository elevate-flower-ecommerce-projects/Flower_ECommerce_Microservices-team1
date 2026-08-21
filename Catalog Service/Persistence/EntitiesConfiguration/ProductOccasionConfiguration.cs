using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class ProductOccasionConfiguration : IEntityTypeConfiguration<ProductOccasion>
{
    public void Configure(EntityTypeBuilder<ProductOccasion> builder)
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
    }
}
