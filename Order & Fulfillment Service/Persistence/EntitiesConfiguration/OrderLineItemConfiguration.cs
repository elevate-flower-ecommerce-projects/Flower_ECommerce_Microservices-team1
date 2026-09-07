using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Persistence.EntitiesConfiguration;

public sealed class OrderLineItemConfiguration : IEntityTypeConfiguration<OrderLineItem>
{
    public void Configure(EntityTypeBuilder<OrderLineItem> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.ProductName).HasMaxLength(220).IsRequired();
        builder.Property(item => item.ThumbnailUrl).HasMaxLength(1000);
        builder.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(item => item.LineTotal).HasColumnType("decimal(18,2)");

        builder.HasIndex(item => item.OrderId);
    }
}