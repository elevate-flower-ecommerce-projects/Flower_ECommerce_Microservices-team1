using Cart_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart_Service.Persistence.EntitiesConfiguration;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.ImageUrl).HasMaxLength(500);
        builder.Property(item => item.UnitPriceSnapshot).HasPrecision(18, 2);

        // A product may appear at most once per cart; adding it again increases the quantity.
        builder.HasIndex(item => new { item.CartId, item.ProductId })
            .IsUnique()
            .HasDatabaseName("UX_CartItem_Cart_Product");
    }
}
