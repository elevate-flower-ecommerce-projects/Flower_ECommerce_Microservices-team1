using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Address___Store_Coverage_Service.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeAddressDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AddressDatabaseInitialization");

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<AddressDbContext>();

            if (app.Environment.IsDevelopment()
                && app.Configuration.GetValue<bool>("DatabaseInitialization:ResetOnStartup"))
            {
                logger.LogWarning("Resetting Address database because DatabaseInitialization:ResetOnStartup is enabled.");
                await context.Database.EnsureDeletedAsync();
            }

            await EnsureDatabaseExistsAsync(context);
            await context.Database.EnsureCreatedAsync();
            await scope.ServiceProvider.GetRequiredService<IAddressDataSeeder>().SeedAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Address database initialization failed. Verify SQL Server is running.");
            throw;
        }
    }

    private static async Task EnsureDatabaseExistsAsync(AddressDbContext context)
    {
        var connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Address database connection string was not found.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("Address database name was not found in the connection string.");

        builder.InitialCatalog = "master";

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
    }
}
