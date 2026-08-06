namespace Identity_service.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task MigrateAndSeedIdentityDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentityDatabaseInitialization");

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>()
                .SeedAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Identity database migration or seeding failed.");
            throw;
        }
    }
}