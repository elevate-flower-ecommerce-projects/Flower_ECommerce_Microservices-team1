using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Order___Fulfillment_Service.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task MigrateOrderDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("OrderDatabaseInitialization");

        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("DatabaseInitialization:ResetOnStartup"))
        {
            logger.LogWarning("Resetting Order database because DatabaseInitialization:ResetOnStartup is enabled.");
            await context.Database.EnsureDeletedAsync();
        }

        var connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Order database connection string was not found.");
        var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "IF DB_ID(@databaseName) IS NULL BEGIN DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@databaseName); EXEC(@sql); END";
        command.Parameters.AddWithValue("@databaseName", databaseName);
        await command.ExecuteNonQueryAsync();

        await context.Database.MigrateAsync();
    }
}
