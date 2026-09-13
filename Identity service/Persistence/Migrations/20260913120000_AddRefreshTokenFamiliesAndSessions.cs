using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity_service.Persistence.Migrations
{
    [Migration("20260913120000_AddRefreshTokenFamiliesAndSessions")]
    public partial class AddRefreshTokenFamiliesAndSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('RefreshTokens', 'CreatedOn') IS NOT NULL AND COL_LENGTH('RefreshTokens', 'IssuedAt') IS NULL
                    EXEC sp_rename 'RefreshTokens.CreatedOn', 'IssuedAt', 'COLUMN';

                IF COL_LENGTH('RefreshTokens', 'ExpiresOn') IS NOT NULL AND COL_LENGTH('RefreshTokens', 'ExpiresAt') IS NULL
                    EXEC sp_rename 'RefreshTokens.ExpiresOn', 'ExpiresAt', 'COLUMN';

                IF COL_LENGTH('RefreshTokens', 'RevokedOn') IS NOT NULL AND COL_LENGTH('RefreshTokens', 'RevokedAt') IS NULL
                    EXEC sp_rename 'RefreshTokens.RevokedOn', 'RevokedAt', 'COLUMN';

                IF COL_LENGTH('RefreshTokens', 'FamilyId') IS NULL
                    ALTER TABLE [RefreshTokens] ADD [FamilyId] uniqueidentifier NOT NULL CONSTRAINT [DF_RefreshTokens_FamilyId] DEFAULT NEWID();

                IF COL_LENGTH('RefreshTokens', 'DeviceInfo') IS NULL
                    ALTER TABLE [RefreshTokens] ADD [DeviceInfo] nvarchar(512) NULL;

                IF COL_LENGTH('RefreshTokens', 'ReplacedByTokenId') IS NULL
                    ALTER TABLE [RefreshTokens] ADD [ReplacedByTokenId] uniqueidentifier NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshTokens_FamilyId' AND object_id = OBJECT_ID(N'RefreshTokens'))
                    CREATE INDEX [IX_RefreshTokens_FamilyId] ON [RefreshTokens] ([FamilyId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshTokens_ReplacedByTokenId' AND object_id = OBJECT_ID(N'RefreshTokens'))
                    CREATE INDEX [IX_RefreshTokens_ReplacedByTokenId] ON [RefreshTokens] ([ReplacedByTokenId]);

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RefreshTokens_RefreshTokens_ReplacedByTokenId')
                    ALTER TABLE [RefreshTokens] ADD CONSTRAINT [FK_RefreshTokens_RefreshTokens_ReplacedByTokenId]
                        FOREIGN KEY ([ReplacedByTokenId]) REFERENCES [RefreshTokens] ([Id]) ON DELETE NO ACTION;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RefreshTokens_RefreshTokens_ReplacedByTokenId')
                    ALTER TABLE [RefreshTokens] DROP CONSTRAINT [FK_RefreshTokens_RefreshTokens_ReplacedByTokenId];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshTokens_ReplacedByTokenId' AND object_id = OBJECT_ID(N'RefreshTokens'))
                    DROP INDEX [IX_RefreshTokens_ReplacedByTokenId] ON [RefreshTokens];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshTokens_FamilyId' AND object_id = OBJECT_ID(N'RefreshTokens'))
                    DROP INDEX [IX_RefreshTokens_FamilyId] ON [RefreshTokens];

                IF COL_LENGTH('RefreshTokens', 'ReplacedByTokenId') IS NOT NULL
                    ALTER TABLE [RefreshTokens] DROP COLUMN [ReplacedByTokenId];

                IF COL_LENGTH('RefreshTokens', 'DeviceInfo') IS NOT NULL
                    ALTER TABLE [RefreshTokens] DROP COLUMN [DeviceInfo];

                IF COL_LENGTH('RefreshTokens', 'FamilyId') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;
                    SELECT @constraintName = [dc].[name]
                    FROM [sys].[default_constraints] [dc]
                    INNER JOIN [sys].[columns] [c] ON [c].[default_object_id] = [dc].[object_id]
                    INNER JOIN [sys].[tables] [t] ON [t].[object_id] = [c].[object_id]
                    WHERE [t].[name] = N'RefreshTokens' AND [c].[name] = N'FamilyId';

                    IF @constraintName IS NOT NULL
                        EXEC(N'ALTER TABLE [RefreshTokens] DROP CONSTRAINT [' + @constraintName + N']');

                    ALTER TABLE [RefreshTokens] DROP COLUMN [FamilyId];
                END

                IF COL_LENGTH('RefreshTokens', 'IssuedAt') IS NOT NULL AND COL_LENGTH('RefreshTokens', 'CreatedOn') IS NULL
                    EXEC sp_rename 'RefreshTokens.IssuedAt', 'CreatedOn', 'COLUMN';

                IF COL_LENGTH('RefreshTokens', 'ExpiresAt') IS NOT NULL AND COL_LENGTH('RefreshTokens', 'ExpiresOn') IS NULL
                    EXEC sp_rename 'RefreshTokens.ExpiresAt', 'ExpiresOn', 'COLUMN';

                IF COL_LENGTH('RefreshTokens', 'RevokedAt') IS NOT NULL AND COL_LENGTH('RefreshTokens', 'RevokedOn') IS NULL
                    EXEC sp_rename 'RefreshTokens.RevokedAt', 'RevokedOn', 'COLUMN';
                """);
        }
    }
}
