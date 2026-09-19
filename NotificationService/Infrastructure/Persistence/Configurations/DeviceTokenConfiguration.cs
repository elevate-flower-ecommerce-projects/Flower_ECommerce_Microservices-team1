using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.HasQueryFilter(dt => !dt.IsDeleted);
    }
}
