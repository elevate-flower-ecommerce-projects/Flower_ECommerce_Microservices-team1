using Identity_service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class DriverDocumentConfiguration : IEntityTypeConfiguration<DriverDocument>
{
    public void Configure(EntityTypeBuilder<DriverDocument> builder)
    {
        #region Table shape

        builder.HasKey(driverDocument => driverDocument.Id);

        builder.Property(driverDocument => driverDocument.FileUrl)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(driverDocument => driverDocument.DocType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(driverDocument => driverDocument.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(driverDocument => driverDocument.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        #endregion

        #region Indexes

        builder.HasIndex(driverDocument => driverDocument.ApplicationId);

        #endregion

        #region Relationships

        builder.HasOne(driverDocument => driverDocument.Application)
            .WithMany(driverApplication => driverApplication.Documents)
            .HasForeignKey(driverDocument => driverDocument.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
