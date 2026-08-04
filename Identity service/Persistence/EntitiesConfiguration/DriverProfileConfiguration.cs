using Identity_service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class DriverProfileConfiguration : IEntityTypeConfiguration<DriverProfile>
{
    public void Configure(EntityTypeBuilder<DriverProfile> builder)
    {
        #region Table shape

        builder.HasKey(driverProfile => driverProfile.Id);

        builder.Property(driverProfile => driverProfile.NationalId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(driverProfile => driverProfile.PlateNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(driverProfile => driverProfile.VehicleType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        #endregion

        #region Indexes

        builder.HasIndex(driverProfile => driverProfile.UserId)
            .IsUnique();

        builder.HasIndex(driverProfile => driverProfile.NationalId)
            .IsUnique();

        #endregion

        #region Relationships

        builder.HasOne(driverProfile => driverProfile.User)
            .WithOne(user => user.DriverProfile)
            .HasForeignKey<DriverProfile>(driverProfile => driverProfile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
