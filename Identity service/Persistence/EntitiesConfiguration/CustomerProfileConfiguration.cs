using Identity_service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        #region Table shape

        builder.HasKey(customerProfile => customerProfile.Id);

        builder.Property(customerProfile => customerProfile.UserId)
            .HasMaxLength(450)
            .IsRequired();

        #endregion

        #region Indexes

        builder.HasIndex(customerProfile => customerProfile.UserId)
            .IsUnique();

        #endregion

        #region Relationships

        builder.HasOne(customerProfile => customerProfile.User)
            .WithOne(user => user.CustomerProfile)
            .HasForeignKey<CustomerProfile>(customerProfile => customerProfile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
