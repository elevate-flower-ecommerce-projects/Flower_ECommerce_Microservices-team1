using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment_Service.Entities;

namespace Payment_Service.Persistence.EntitiesConfiguration;

public sealed class ProcessedPaymentEventConfiguration : IEntityTypeConfiguration<ProcessedPaymentEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedPaymentEvent> builder)
    {
        builder.HasKey(processed => processed.Id);
        builder.Property(processed => processed.Provider).HasMaxLength(32).IsRequired();
        builder.Property(processed => processed.EventId).HasMaxLength(255).IsRequired();
        builder.Property(processed => processed.EventType).HasMaxLength(100).IsRequired();

        // A redelivered webhook hits this index and is dropped instead of being acted on twice.
        builder.HasIndex(processed => new { processed.Provider, processed.EventId })
            .IsUnique()
            .HasDatabaseName("UX_ProcessedPaymentEvent_Provider_EventId");
    }
}
