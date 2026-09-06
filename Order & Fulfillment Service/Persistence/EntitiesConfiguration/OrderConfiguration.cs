using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Persistence.EntitiesConfiguration;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.Property(order => order.OrderNumber).HasMaxLength(64).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.Property(order => order.CustomerUserId).HasMaxLength(450).IsRequired();
        builder.Property(order => order.AssignedDriverUserId).HasMaxLength(450);
        builder.HasIndex(order => new { order.AssignedDriverUserId, order.CreatedAtUtc });
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(order => order.StoreName).HasMaxLength(160).IsRequired();
        builder.Property(order => order.StoreAddress).HasMaxLength(500).IsRequired();
        builder.Property(order => order.RecipientName).HasMaxLength(120).IsRequired();
        builder.Property(order => order.RecipientPhone).HasMaxLength(30).IsRequired();
        builder.Property(order => order.DeliveryAddressLine).HasMaxLength(500).IsRequired();
        builder.Property(order => order.DeliveryCity).HasMaxLength(120).IsRequired();
        builder.Property(order => order.DeliveryArea).HasMaxLength(120).IsRequired();
        builder.Property(order => order.StoreLatitude).HasPrecision(9, 6);
        builder.Property(order => order.StoreLongitude).HasPrecision(9, 6);
        builder.Property(order => order.DeliveryLatitude).HasPrecision(9, 6);
        builder.Property(order => order.DeliveryLongitude).HasPrecision(9, 6);
    }
}
