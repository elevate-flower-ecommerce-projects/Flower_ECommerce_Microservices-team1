using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.HasKey(banner => banner.Id);

        builder.Property(banner => banner.ImageUrl)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(banner => banner.DeepLink)
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(banner => banner.StoreId);
        builder.HasIndex(banner => banner.SortOrder);
    }
}
