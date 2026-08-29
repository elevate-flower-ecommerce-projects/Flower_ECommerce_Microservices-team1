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
            await context.Database.EnsureCreatedAsync();
            await EnsureAddressSchemaAsync(context);
            await EnsureStoreSchemaAsync(context);
            await scope.ServiceProvider.GetRequiredService<IAddressDataSeeder>().SeedAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Address database initialization failed. Verify SQL Server is running.");
            throw;
        }
    }

    private static async Task EnsureAddressSchemaAsync(AddressDbContext context)
    {
        var sql = """
            IF OBJECT_ID(N'[UserAddresses]', N'U') IS NOT NULL
                AND COL_LENGTH(N'[UserAddresses]', N'LastUsedAtUtc') IS NULL
            BEGIN
                ALTER TABLE [UserAddresses]
                ADD [LastUsedAtUtc] datetime2 NULL;
            END;
            """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }
    private static async Task EnsureStoreSchemaAsync(AddressDbContext context)
    {
        var sql = """
            IF OBJECT_ID(N'[Stores]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Stores] (
                    [Id] uniqueidentifier NOT NULL,
                    [Name] nvarchar(160) NOT NULL,
                    [Location] nvarchar(500) NOT NULL,
                    [Lat] decimal(9,6) NULL,
                    [Lng] decimal(9,6) NULL,
                    [IsActive] bit NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL,
                    [UpdatedAtUtc] datetime2 NULL,
                    CONSTRAINT [PK_Stores] PRIMARY KEY ([Id])
                );

                CREATE INDEX [IX_Stores_IsActive] ON [Stores] ([IsActive]);
                CREATE INDEX [IX_Stores_Name] ON [Stores] ([Name]);
            END;

            IF COL_LENGTH(N'[StoreCoverageAreas]', N'StoreId') IS NOT NULL
                AND OBJECT_ID(N'[FK_StoreCoverageAreas_Stores_StoreId]', N'F') IS NULL
                AND OBJECT_ID(N'[Stores]', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM [StoreCoverageAreas] coverage
                    WHERE NOT EXISTS (SELECT 1 FROM [Stores] store WHERE store.[Id] = coverage.[StoreId])
                )
                BEGIN
                    ALTER TABLE [StoreCoverageAreas]
                    ADD CONSTRAINT [FK_StoreCoverageAreas_Stores_StoreId]
                    FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]);
                END
            END;
            """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
