using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class AdminSecurityAuditConfiguration : IEntityTypeConfiguration<AdminSecurityAudit>
{
    public void Configure(EntityTypeBuilder<AdminSecurityAudit> builder)
    {
        builder.ToTable("AdminSecurityAudits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(64).IsRequired(); 
        builder.Property(x => x.Outcome).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.Path).HasMaxLength(512);
        builder.HasIndex(x => x.OccurredOnUtc);
    }
}
