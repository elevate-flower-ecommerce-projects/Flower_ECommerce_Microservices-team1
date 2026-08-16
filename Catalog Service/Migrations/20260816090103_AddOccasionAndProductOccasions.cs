using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddOccasionAndProductOccasions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductOccasions')
                BEGIN
                    CREATE TABLE [ProductOccasions] (
                        [ProductId] uniqueidentifier NOT NULL,
                        [OccasionId] uniqueidentifier NOT NULL,
                        CONSTRAINT [PK_ProductOccasions] PRIMARY KEY ([ProductId], [OccasionId]),
                        CONSTRAINT [FK_ProductOccasions_Occasions_OccasionId] FOREIGN KEY ([OccasionId]) REFERENCES [Occasions] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ProductOccasions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_ProductOccasions_OccasionId] ON [ProductOccasions] ([OccasionId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductOccasions')
                BEGIN
                    DROP TABLE [ProductOccasions];
                END
            ");
        }
    }
}
