using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MainBackend.Migrations
{
    /// <inheritdoc />
    public partial class addBlogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Blogs",
                columns: new[] { "Id", "CreatedDate", "Description", "Image", "IsDeleted", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 3, 22, 51, 33, 792, DateTimeKind.Local).AddTicks(138), "Class aptent taciti sociosqu ad litora torquent per conubia nostra, per", "blog-feature-img-1.jpg", false, "Flower Power" },
                    { 2, new DateTime(2026, 1, 3, 22, 51, 33, 793, DateTimeKind.Local).AddTicks(529), "This is the first post in our blog. Stay tuned for updates!", "blog-feature-img-3.jpg", false, "Local Florists" },
                    { 3, new DateTime(2026, 1, 3, 22, 51, 33, 793, DateTimeKind.Local).AddTicks(540), "This is the first post in our blog. Stay tuned for updates!", "blog-feature-img-4.jpg", false, "Flower Beauty" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blogs");
        }
    }
}
