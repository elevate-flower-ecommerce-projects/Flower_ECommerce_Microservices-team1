using Identity_service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Infrastructure.Persistence.Configurations;

public class VerificationCodeConfiguration : IEntityTypeConfiguration<VerificationCode>
{
    public void Configure(EntityTypeBuilder<VerificationCode> builder)
    {
        builder.ToTable("VerificationCodes");

        builder.HasKey(vc => vc.Id);

        builder.Property(vc => vc.CodeHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(vc => vc.Purpose)
            .HasConversion<int>();

        builder.HasIndex(vc => new { vc.UserId, vc.Purpose });

        builder.Ignore(vc => vc.IsExpired);
        builder.Ignore(vc => vc.IsActive);
    }
}
