using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RentalApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DailyPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_item_users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_image",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_image", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_image_item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rental",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    RenterId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rental", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rental_item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rental_users_RenterId",
                        column: x => x.RenterId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rating",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RentalId = table.Column<int>(type: "integer", nullable: false),
                    RaterId = table.Column<int>(type: "integer", nullable: false),
                    RatedUserId = table.Column<int>(type: "integer", nullable: false),
                    Stars = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rating", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rating_rental_RentalId",
                        column: x => x.RentalId,
                        principalTable: "rental",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rating_users_RatedUserId",
                        column: x => x.RatedUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rating_users_RaterId",
                        column: x => x.RaterId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_Name",
                table: "category",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_CategoryId",
                table: "item",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_item_CreatedDate",
                table: "item",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_item_IsAvailable",
                table: "item",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_item_OwnerId",
                table: "item",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_item_image_ItemId",
                table: "item_image",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_item_image_ItemId_IsPrimary",
                table: "item_image",
                columns: new[] { "ItemId", "IsPrimary" },
                filter: "\"IsPrimary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_rating_RatedUserId",
                table: "rating",
                column: "RatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_rating_RaterId",
                table: "rating",
                column: "RaterId");

            migrationBuilder.CreateIndex(
                name: "IX_rating_RentalId",
                table: "rating",
                column: "RentalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rental_EndDate",
                table: "rental",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_rental_ItemId",
                table: "rental",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_rental_RenterId",
                table: "rental",
                column: "RenterId");

            migrationBuilder.CreateIndex(
                name: "IX_rental_StartDate",
                table: "rental",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_rental_Status",
                table: "rental",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_image");

            migrationBuilder.DropTable(
                name: "rating");

            migrationBuilder.DropTable(
                name: "rental");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "category");
        }
    }
}
