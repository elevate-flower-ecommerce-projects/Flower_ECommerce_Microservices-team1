using Address___Store_Coverage_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Address___Store_Coverage_Service.Persistence.EntitiesConfiguration;

public sealed class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.HasKey(address => address.Id);

        builder.Property(address => address.UserId).HasMaxLength(450).IsRequired();
        builder.Property(address => address.RecipientName).HasMaxLength(120).IsRequired();
        builder.Property(address => address.Phone).HasMaxLength(11).IsRequired();
        builder.Property(address => address.AddressLine).HasMaxLength(500).IsRequired();
        builder.Property(address => address.City).HasMaxLength(120).IsRequired();
        builder.Property(address => address.Area).HasMaxLength(120).IsRequired();
        builder.Property(address => address.Label).HasMaxLength(50);
        builder.Property(address => address.Lat).HasPrecision(9, 6);
        builder.Property(address => address.Lng).HasPrecision(9, 6);

        builder.HasIndex(address => address.UserId);
        builder.HasIndex(address => address.ServingStoreId);
        builder.HasIndex(address => new { address.UserId, address.IsDefault });
        builder.HasIndex(address => new { address.UserId, address.LastUsedAtUtc, address.CreatedAtUtc });
    }
}
