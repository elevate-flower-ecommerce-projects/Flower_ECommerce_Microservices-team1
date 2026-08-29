using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Cart_Service.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task MigrateCartDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("CartDatabaseInitialization");

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<CartDbContext>();

            if (app.Environment.IsDevelopment()
                && app.Configuration.GetValue<bool>("DatabaseInitialization:ResetOnStartup"))
            {
                logger.LogWarning("Resetting Cart database because DatabaseInitialization:ResetOnStartup is enabled.");
                await context.Database.EnsureDeletedAsync();
            }

            await EnsureDatabaseExistsAsync(context, logger);
            await MigrateAsync(context, logger);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Cart database migration failed. Verify SQL Server is running.");
            throw;
        }
    }

    private static async Task EnsureDatabaseExistsAsync(CartDbContext context, ILogger logger)
    {
        var connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Cart database connection string was not found.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Cart database name was not found in the connection string.");
        }

        builder.InitialCatalog = "master";

        const int maxRetries = 5;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = """
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
                        "Cart database already exists while ensuring it exists. Continuing with migrations.");
                }

                break;
            }
            catch (SqlException ex) when (attempt < maxRetries)
            {
                logger.LogWarning(
                    ex,
                    "Failed to connect to SQL Server (Attempt {Attempt}/{MaxRetries}). Retrying in 3 seconds...",
                    attempt,
                    maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    private static async Task MigrateAsync(CartDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (SqlException exception) when (exception.Number == 1801)
        {
            logger.LogWarning(
                exception,
                "Cart database already exists during migration startup. Waiting until the existing database is available.");

            await WaitUntilDatabaseCanConnectAsync(context, logger);
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (!pendingMigrations.Any())
            {
                logger.LogInformation("Cart database already exists and has no pending migrations.");
                return;
            }

            await context.Database.MigrateAsync();
        }
    }

    private static async Task WaitUntilDatabaseCanConnectAsync(CartDbContext context, ILogger logger)
    {
        const int maxRetries = 5;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            if (await context.Database.CanConnectAsync())
            {
                return;
            }

            logger.LogWarning(
                "Cart database exists but is not connectable yet (Attempt {Attempt}/{MaxRetries}). Retrying in 3 seconds...",
                attempt,
                maxRetries);

            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        throw new InvalidOperationException("Cart database exists but could not be reached for migrations.");
    }
}
