using Identity_service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        builder.HasIndex(rt => rt.UserId);

        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);
    }
}
