using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity_service.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('DriverProfiles', 'Country') IS NULL
                BEGIN
                    ALTER TABLE [DriverProfiles] ADD [Country] nvarchar(100) NOT NULL CONSTRAINT [DF_DriverProfiles_Country] DEFAULT N'Egypt';
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('AspNetUsers', 'ProfilePictureUrl') IS NULL
                BEGIN
                    ALTER TABLE [AspNetUsers] ADD [ProfilePictureUrl] nvarchar(512) NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('DriverProfiles', 'Country') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;
                    SELECT @constraintName = [dc].[name]
                    FROM [sys].[default_constraints] [dc]
                    INNER JOIN [sys].[columns] [c] ON [c].[default_object_id] = [dc].[object_id]
                    INNER JOIN [sys].[tables] [t] ON [t].[object_id] = [c].[object_id]
                    WHERE [t].[name] = N'DriverProfiles' AND [c].[name] = N'Country';

                    IF @constraintName IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [DriverProfiles] DROP CONSTRAINT [' + @constraintName + N']');
                    END

                    ALTER TABLE [DriverProfiles] DROP COLUMN [Country];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('AspNetUsers', 'ProfilePictureUrl') IS NOT NULL
                BEGIN
                    ALTER TABLE [AspNetUsers] DROP COLUMN [ProfilePictureUrl];
                END
                """);
        }
    }
}