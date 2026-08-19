using Address___Store_Coverage_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Address___Store_Coverage_Service.Persistence.EntitiesConfiguration;

public sealed class StoreCoverageAreaConfiguration : IEntityTypeConfiguration<StoreCoverageArea>
{
    public void Configure(EntityTypeBuilder<StoreCoverageArea> builder)
    {
        builder.HasKey(coverage => coverage.Id);

        builder.Property(coverage => coverage.City).HasMaxLength(120).IsRequired();
        builder.Property(coverage => coverage.Area).HasMaxLength(120).IsRequired();
        builder.Property(coverage => coverage.MinLat).HasPrecision(9, 6);
        builder.Property(coverage => coverage.MaxLat).HasPrecision(9, 6);
        builder.Property(coverage => coverage.MinLng).HasPrecision(9, 6);
        builder.Property(coverage => coverage.MaxLng).HasPrecision(9, 6);

        builder.HasIndex(coverage => new { coverage.City, coverage.Area });
        builder.HasIndex(coverage => coverage.StoreId);
    }
}
