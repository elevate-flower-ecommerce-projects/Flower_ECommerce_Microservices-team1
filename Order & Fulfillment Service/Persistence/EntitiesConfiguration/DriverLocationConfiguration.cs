using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Persistence.EntitiesConfiguration;

public sealed class DriverLocationConfiguration : IEntityTypeConfiguration<DriverLocation>
{
    public void Configure(EntityTypeBuilder<DriverLocation> builder)
    {
        builder.HasKey(location => location.Id);
        builder.Property(location => location.DriverUserId).HasMaxLength(450).IsRequired();
        builder.Property(location => location.Latitude).HasPrecision(9, 6);
        builder.Property(location => location.Longitude).HasPrecision(9, 6);
        builder.HasIndex(location => new { location.OrderId, location.RecordedAtUtc });
        builder.HasOne(location => location.Order)
            .WithMany(order => order.DriverLocations)
            .HasForeignKey(location => location.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
