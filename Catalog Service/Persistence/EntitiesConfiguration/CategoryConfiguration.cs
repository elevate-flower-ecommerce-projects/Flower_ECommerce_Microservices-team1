using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Service.Persistence.EntitiesConfiguration;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(category => category.ImageUrl)
            .HasMaxLength(512);

        builder.HasIndex(category => category.SortOrder);
    }
}
