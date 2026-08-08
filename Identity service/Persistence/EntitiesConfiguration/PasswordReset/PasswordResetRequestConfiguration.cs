using Identity_service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public sealed class PasswordResetRequestConfiguration : IEntityTypeConfiguration<PasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<PasswordResetRequest> builder)
    {
        builder.ToTable("PasswordResetRequests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.UserId).HasMaxLength(450).IsRequired();
        builder.Property(request => request.OtpHash).HasMaxLength(64).IsRequired();
        builder.Property(request => request.ResetTokenHash).HasMaxLength(64);
        builder.HasIndex(request => request.UserId);
        builder.HasIndex(request => new { request.UserId, request.CreatedAtUtc });
        builder.HasOne(request => request.User).WithMany(user => user.PasswordResetRequests).HasForeignKey(request => request.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
