using Address___Store_Coverage_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Address___Store_Coverage_Service.Persistence.EntitiesConfiguration;

public sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(store => store.Id);

        builder.Property(store => store.Name).HasMaxLength(160).IsRequired();
        builder.Property(store => store.Location).HasMaxLength(500).IsRequired();
        builder.Property(store => store.Lat).HasPrecision(9, 6);
        builder.Property(store => store.Lng).HasPrecision(9, 6);

        builder.HasIndex(store => store.Name);
        builder.HasIndex(store => store.IsActive);
    }
}
