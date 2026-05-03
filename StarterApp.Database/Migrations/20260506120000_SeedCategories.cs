using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentalApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "category",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Electronic devices and gadgets", "Electronics" },
                    { 2, "Power tools, hand tools, and equipment", "Tools" },
                    { 9, "Camping gear and outdoor equipment", "Camping & Outdoor" },
                    { 10, "Miscellaneous items", "Other" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "category",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    9,
                    10
                });
        }
    }
}
