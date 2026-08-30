using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task MigrateAndSeedCatalogDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("CatalogDatabaseInitialization");

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            if (app.Environment.IsDevelopment()
                && app.Configuration.GetValue<bool>("DatabaseInitialization:ResetOnStartup"))
            {
                logger.LogWarning("Resetting Catalog database because DatabaseInitialization:ResetOnStartup is enabled.");
                await context.Database.EnsureDeletedAsync();
            }
            await MigrateAsync(context, logger);
            await scope.ServiceProvider.GetRequiredService<ICatalogDataSeeder>().SeedAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Catalog database migration or seeding failed. Verify SQL Server is running.");
            throw;
        }
    }

    private static async Task MigrateAsync(
        CatalogDbContext context,
        ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (SqlException exception) when (exception.Number == 1801)
        {
            logger.LogWarning(
                exception,
                "Catalog database already exists during migration startup. Waiting until the existing database is available.");

            await WaitUntilDatabaseCanConnectAsync(context, logger);
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (!pendingMigrations.Any())
            {
                logger.LogInformation("Catalog database already exists and has no pending migrations.");
                return;
            }

            await context.Database.MigrateAsync();
        }
    }

    private static async Task WaitUntilDatabaseCanConnectAsync(
        CatalogDbContext context,
        ILogger logger)
    {
        const int maxRetries = 5;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            if (await context.Database.CanConnectAsync())
            {
                return;
            }

            logger.LogWarning(
                "Catalog database exists but is not connectable yet (Attempt {Attempt}/{MaxRetries}). Retrying in 3 seconds...",
                attempt,
                maxRetries);

            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        throw new InvalidOperationException("Catalog database exists but could not be reached for migrations.");
    }
}
