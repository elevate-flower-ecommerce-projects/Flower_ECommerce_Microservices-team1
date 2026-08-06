using Identity_service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public sealed class PasswordResetAuditEventConfiguration : IEntityTypeConfiguration<PasswordResetAuditEvent>
{
    public void Configure(EntityTypeBuilder<PasswordResetAuditEvent> builder)
    {
        builder.ToTable("PasswordResetAuditEvents");
        builder.HasKey(eventItem => eventItem.Id);
        builder.Property(eventItem => eventItem.UserId).HasMaxLength(450).IsRequired();
        builder.Property(eventItem => eventItem.EventType).HasMaxLength(100).IsRequired();
        builder.HasIndex(eventItem => eventItem.UserId);
        builder.HasIndex(eventItem => eventItem.ResetRequestId);
        builder.HasOne(eventItem => eventItem.User).WithMany(user => user.PasswordResetAuditEvents).HasForeignKey(eventItem => eventItem.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(eventItem => eventItem.ResetRequest).WithMany(request => request.AuditEvents).HasForeignKey(eventItem => eventItem.ResetRequestId).OnDelete(DeleteBehavior.Cascade);
    }
}
