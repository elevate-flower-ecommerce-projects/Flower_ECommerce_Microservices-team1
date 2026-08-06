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

        var passwordHasher = new PasswordHasher<ApplicationUser>().HashPassword(null, DefaultUsers.Admin.Password);

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
            PasswordHash = passwordHasher,
            SecurityStamp = DefaultUsers.Admin.SecurityStamp,
            ConcurrencyStamp = DefaultUsers.Admin.ConcurrencyStamp,
        };

        builder.HasData(adminUser);
    }
}