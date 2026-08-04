using Identity_service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class DriverApplicationConfiguration : IEntityTypeConfiguration<DriverApplication>
{
    public void Configure(EntityTypeBuilder<DriverApplication> builder)
    {
        #region Table shape

        builder.HasKey(driverApplication => driverApplication.Id);

        builder.Property(driverApplication => driverApplication.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(driverApplication => driverApplication.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(driverApplication => driverApplication.ReviewedBy)
            .HasMaxLength(450);

        #endregion

        #region Indexes

        builder.HasIndex(driverApplication => driverApplication.UserId)
            .IsUnique();

        #endregion

        #region Relationships

        builder.HasOne(driverApplication => driverApplication.User)
            .WithMany(user => user.DriverApplications)
            .HasForeignKey(driverApplication => driverApplication.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
