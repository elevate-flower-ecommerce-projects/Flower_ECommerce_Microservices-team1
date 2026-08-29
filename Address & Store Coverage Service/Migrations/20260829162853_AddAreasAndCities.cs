using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Address___Store_Coverage_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddAreasAndCities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ServingStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsServiceable = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoreCoverageAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MinLat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    MaxLat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    MinLng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    MaxLng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCoverageAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreCoverageAreas_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Name",
                table: "Areas",
                column: "Name",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_AreaId_Name",
                table: "Cities",
                columns: new[] { "AreaId", "Name" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCoverageAreas_City_Area",
                table: "StoreCoverageAreas",
                columns: new[] { "City", "Area" });

            migrationBuilder.CreateIndex(
                name: "IX_StoreCoverageAreas_City_Area_IsActive",
                table: "StoreCoverageAreas",
                columns: new[] { "City", "Area", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_StoreCoverageAreas_IsActive",
                table: "StoreCoverageAreas",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCoverageAreas_StoreId",
                table: "StoreCoverageAreas",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsActive",
                table: "Stores",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Name",
                table: "Stores",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_ServingStoreId",
                table: "UserAddresses",
                column: "ServingStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId",
                table: "UserAddresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId_IsDefault",
                table: "UserAddresses",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId_LastUsedAtUtc_CreatedAtUtc",
                table: "UserAddresses",
                columns: new[] { "UserId", "LastUsedAtUtc", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "StoreCoverageAreas");

            migrationBuilder.DropTable(
                name: "UserAddresses");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Stores");
        }
    }
}
