using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class ProductStoreInventoryConfiguration : IEntityTypeConfiguration<ProductStoreInventory>
{
    public void Configure(EntityTypeBuilder<ProductStoreInventory> builder)
    {
        builder.HasKey(inventory => inventory.Id);

        builder.HasIndex(inventory => new { inventory.ProductId, inventory.StoreId })
            .IsUnique();

        builder.HasIndex(inventory => inventory.StoreId);

        builder.HasOne(inventory => inventory.Product)
            .WithMany(product => product.StoreInventories)
            .HasForeignKey(inventory => inventory.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
