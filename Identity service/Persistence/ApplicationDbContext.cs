using Identity_service.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Identity_service.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<DriverApplication> DriverApplications => Set<DriverApplication>();
    public DbSet<DriverDocument> DriverDocuments => Set<DriverDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity's own mapping must be applied first, otherwise it overwrites
        // anything our configurations set on the Identity entities.
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
