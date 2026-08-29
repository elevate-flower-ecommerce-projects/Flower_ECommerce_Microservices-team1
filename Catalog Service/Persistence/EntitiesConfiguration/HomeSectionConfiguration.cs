using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class HomeSectionConfiguration : IEntityTypeConfiguration<HomeSection>
{
    public void Configure(EntityTypeBuilder<HomeSection> builder)
    {
        builder.ToTable("HomeSection");

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Type)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(section => section.Title)
            .HasMaxLength(160);

        builder.Property(section => section.TitleAr)
            .HasMaxLength(160);

        builder.Property(section => section.ContentRefJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasOne(section => section.Occasion)
            .WithMany()
            .HasForeignKey(section => section.OccasionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(section => section.Category)
            .WithMany()
            .HasForeignKey(section => section.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(section => section.Order);
    }
}
