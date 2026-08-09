using Identity_service.Abstractions.Seeding;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(user => user.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(32);

        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique()
            .HasDatabaseName("UX_ApplicationUser_PhoneNumber")
            .HasFilter("[PhoneNumber] IS NOT NULL");

        var adminUser = new ApplicationUser
        {
            Id = DefaultUsers.Admin.Id,
            FirstName = DefaultUsers.Admin.FirstName,
            LastName = DefaultUsers.Admin.LastName,
            Email = DefaultUsers.Admin.Email,
            NormalizedEmail = DefaultUsers.Admin.Email.ToUpper(),
            UserName = DefaultUsers.Admin.Email,
            NormalizedUserName = DefaultUsers.Admin.Email.ToUpper(),
            EmailConfirmed = true,
            LockoutEnabled = true,
            PasswordHash = DefaultUsers.Admin.PasswordHash,
            SecurityStamp = DefaultUsers.Admin.SecurityStamp,
            ConcurrencyStamp = DefaultUsers.Admin.ConcurrencyStamp,
        };

        builder.HasData(adminUser);
    }
}
