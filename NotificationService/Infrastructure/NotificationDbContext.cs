using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Infrastructure;

public class NotificationDbContext:DbContext
{
    public DbSet<DeviceToken> Notifications { get; set; }
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
