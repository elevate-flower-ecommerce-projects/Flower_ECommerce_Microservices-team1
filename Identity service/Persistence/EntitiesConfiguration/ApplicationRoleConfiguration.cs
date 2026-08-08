using Identity_service.Abstractions.Seeding;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity_service.Persistence.EntitiesConfiguration;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        var adminRole = new ApplicationRole
        {
            Id = DefaultRoles.Admin.Id,
            Name = DefaultRoles.Admin.Name,
            NormalizedName = DefaultRoles.Admin.Name.ToUpper(),
            ConcurrencyStamp = DefaultRoles.Admin.ConcurrencyStamp,
        };

        var customerRole = new ApplicationRole
        {
            Id = DefaultRoles.Customer.Id,
            Name = DefaultRoles.Customer.Name,
            NormalizedName = DefaultRoles.Customer.Name.ToUpper(),
            ConcurrencyStamp = DefaultRoles.Customer.ConcurrencyStamp,
        };

        builder.HasData(adminRole, customerRole);
    }
}
