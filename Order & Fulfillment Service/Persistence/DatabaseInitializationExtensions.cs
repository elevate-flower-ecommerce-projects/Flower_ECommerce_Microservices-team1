using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Order___Fulfillment_Service.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeOrderDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("OrderDatabaseInitialization");

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            if (app.Environment.IsDevelopment()
                && app.Configuration.GetValue<bool>("DatabaseInitialization:ResetOnStartup"))
            {
                logger.LogWarning("Resetting Order database because DatabaseInitialization:ResetOnStartup is enabled.");
                await context.Database.EnsureDeletedAsync();
            }

            await EnsureDatabaseExistsAsync(context, logger);
            await context.Database.EnsureCreatedAsync();
            await scope.ServiceProvider.GetRequiredService<IOrderDataSeeder>().SeedAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Order database initialization failed. Verify SQL Server is running.");
            throw;
        }
    }

    private static async Task EnsureDatabaseExistsAsync(OrderDbContext context, ILogger logger)
    {
        var connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Order database connection string was not found.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("Order database name was not found in the connection string.");

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
                await command.ExecuteNonQueryAsync();
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
}