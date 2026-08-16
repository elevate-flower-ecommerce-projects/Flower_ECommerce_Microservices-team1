using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class OccasionConfiguration : IEntityTypeConfiguration<Occasion>
{
    public void Configure(EntityTypeBuilder<Occasion> builder)
    {
        builder.HasKey(occasion => occasion.Id);

        builder.Property(occasion => occasion.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(occasion => occasion.ImageUrl)
            .HasMaxLength(512);

        builder.HasIndex(occasion => occasion.SortOrder);
    }
}
