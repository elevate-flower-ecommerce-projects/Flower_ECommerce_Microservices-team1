using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Persistence.EntitiesConfiguration;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);

        builder.Property(order => order.UserId).HasMaxLength(64).IsRequired();
        builder.Property(order => order.OrderNumber).HasMaxLength(32).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(order => order.DeliveryRecipientName).HasMaxLength(160).IsRequired();
        builder.Property(order => order.DeliveryPhone).HasMaxLength(32).IsRequired();
        builder.Property(order => order.DeliveryAddressLine).HasMaxLength(500).IsRequired();
        builder.Property(order => order.DeliveryCity).HasMaxLength(120).IsRequired();
        builder.Property(order => order.DeliveryArea).HasMaxLength(120).IsRequired();
        builder.Property(order => order.GiftRecipientName).HasMaxLength(160);
        builder.Property(order => order.GiftRecipientPhone).HasMaxLength(32);
        builder.Property(order => order.GiftMessage).HasMaxLength(500);
        builder.Property(order => order.PaymentMethod).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(order => order.PaymentLast4).HasMaxLength(4);
        builder.Property(order => order.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(order => order.DeliveryFee).HasColumnType("decimal(18,2)");
        builder.Property(order => order.Discount).HasColumnType("decimal(18,2)");
        builder.Property(order => order.Total).HasColumnType("decimal(18,2)");

        builder.HasIndex(order => new { order.UserId, order.PlacedAtUtc });
        builder.HasIndex(order => order.OrderNumber).IsUnique();

        builder.HasMany(order => order.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}