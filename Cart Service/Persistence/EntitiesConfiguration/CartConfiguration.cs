using Cart_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart_Service.Persistence.EntitiesConfiguration;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.UserId).HasMaxLength(450).IsRequired();

        builder.HasIndex(cart => cart.UserId)
            .IsUnique()
            .HasDatabaseName("UX_Cart_UserId");

        builder.HasMany(cart => cart.Items)
            .WithOne(item => item.Cart)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
