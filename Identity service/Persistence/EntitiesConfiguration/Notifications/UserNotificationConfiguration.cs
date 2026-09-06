using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration.Notifications;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(notification => notification.Title)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(notification => notification.Body)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(notification => notification.Type)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(notification => notification.DeepLink)
            .HasMaxLength(512);

        builder.HasIndex(notification => new { notification.UserId, notification.CreatedAtUtc });
        builder.HasIndex(notification => new { notification.UserId, notification.IsRead });

        builder.HasOne(notification => notification.User)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}