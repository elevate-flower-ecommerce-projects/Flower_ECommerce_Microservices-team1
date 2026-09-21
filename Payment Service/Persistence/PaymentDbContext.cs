using Microsoft.EntityFrameworkCore;
using Payment_Service.Entities;

namespace Payment_Service.Persistence;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<ProcessedPaymentEvent> ProcessedPaymentEvents => Set<ProcessedPaymentEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
}
