using Microsoft.Data.SqlClient;

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

            if (app.Environment.IsDevelopment()
                && app.Configuration.GetValue<bool>("DatabaseInitialization:ResetOnStartup"))
            {
                logger.LogWarning("Resetting Identity database because DatabaseInitialization:ResetOnStartup is enabled.");
                await context.Database.EnsureDeletedAsync();
            }

            await EnsureDatabaseExistsAsync(context, logger);
            await MigrateAsync(context, logger);
            await scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>()
                .SeedAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Identity database migration or seeding failed. Verify SQL Server is running.");
        }
    }

    private static async Task EnsureDatabaseExistsAsync(
        ApplicationDbContext context,
        ILogger logger)
    {
        var connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Identity database connection string was not found.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Identity database name was not found in the connection string.");
        }

        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(@databaseName) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@databaseName);
                EXEC(@sql);
            END
            """;
        command.Parameters.AddWithValue("@databaseName", databaseName);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException exception) when (exception.Number == 1801)
        {
            logger.LogWarning(
                exception,
                "Identity database already exists while ensuring it exists. Continuing with migrations.");
        }
    }

    private static async Task MigrateAsync(
        ApplicationDbContext context,
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
                "Identity database already exists during migration startup. Waiting until the existing database is available.");

            await WaitUntilDatabaseCanConnectAsync(context, logger);
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (!pendingMigrations.Any())
            {
                logger.LogInformation("Identity database already exists and has no pending migrations.");
                return;
            }

            await context.Database.MigrateAsync();
        }
    }

    private static async Task WaitUntilDatabaseCanConnectAsync(
        ApplicationDbContext context,
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
                "Identity database exists but is not connectable yet (Attempt {Attempt}/{MaxRetries}). Retrying in 3 seconds...",
                attempt,
                maxRetries);

            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        throw new InvalidOperationException("Identity database exists but could not be reached for migrations.");
    }
}
