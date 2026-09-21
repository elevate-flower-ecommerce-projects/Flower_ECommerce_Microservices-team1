using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payment_Service.Entities;

namespace Payment_Service.Persistence;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<ProcessedPaymentEvent> ProcessedPaymentEvents => Set<ProcessedPaymentEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);

    // datetime2 stores no time zone, so values come back as Unspecified and serialize without a
    // "Z", which a client may read as its own local time. Everything here is written in UTC.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        => configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
