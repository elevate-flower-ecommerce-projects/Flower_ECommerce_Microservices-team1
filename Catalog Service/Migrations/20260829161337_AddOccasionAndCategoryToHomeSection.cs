using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddOccasionAndCategoryToHomeSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "HomeSection",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OccasionId",
                table: "HomeSection",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "HomeSection",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeSection_CategoryId",
                table: "HomeSection",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeSection_OccasionId",
                table: "HomeSection",
                column: "OccasionId");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeSection_Categories_CategoryId",
                table: "HomeSection",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HomeSection_Occasions_OccasionId",
                table: "HomeSection",
                column: "OccasionId",
                principalTable: "Occasions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeSection_Categories_CategoryId",
                table: "HomeSection");

            migrationBuilder.DropForeignKey(
                name: "FK_HomeSection_Occasions_OccasionId",
                table: "HomeSection");

            migrationBuilder.DropIndex(
                name: "IX_HomeSection_CategoryId",
                table: "HomeSection");

            migrationBuilder.DropIndex(
                name: "IX_HomeSection_OccasionId",
                table: "HomeSection");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "HomeSection");

            migrationBuilder.DropColumn(
                name: "OccasionId",
                table: "HomeSection");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "HomeSection");
        }
    }
}
