using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RentalApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPostGISLocationToItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "item",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "item",
                type: "geography (point)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_Location",
                table: "item",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_item_Location",
                table: "item");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "item");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "item");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
