using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment_Service.Entities;

namespace Payment_Service.Persistence.EntitiesConfiguration;

public sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.OrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(attempt => attempt.CustomerUserId).HasMaxLength(450).IsRequired();
        builder.Property(attempt => attempt.Amount).HasPrecision(18, 2);
        builder.Property(attempt => attempt.Currency).HasMaxLength(3).IsRequired();
        builder.Property(attempt => attempt.Provider).HasMaxLength(32).IsRequired();
        builder.Property(attempt => attempt.ProviderSessionId).HasMaxLength(255);
        builder.Property(attempt => attempt.ProviderPaymentIntentId).HasMaxLength(255);
        builder.Property(attempt => attempt.CheckoutUrl).HasMaxLength(2048);
        builder.Property(attempt => attempt.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(attempt => attempt.FailureReason).HasMaxLength(500);

        // The webhook arrives with a session id and has to find its attempt by it.
        builder.HasIndex(attempt => attempt.ProviderSessionId)
            .IsUnique()
            .HasFilter("[ProviderSessionId] IS NOT NULL")
            .HasDatabaseName("UX_PaymentAttempt_ProviderSessionId");

        // Latest attempt for an order, for the status endpoint and for reusing an open session.
        builder.HasIndex(attempt => new { attempt.OrderId, attempt.CreatedAtUtc })
            .HasDatabaseName("IX_PaymentAttempt_Order_CreatedAt");

        // An order can only be paid once, however many attempts it took.
        builder.HasIndex(attempt => attempt.OrderId)
            .IsUnique()
            .HasFilter("[Status] = 'Succeeded'")
            .HasDatabaseName("UX_PaymentAttempt_Order_Succeeded");
    }
}
