using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MaleFashion.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroSlideEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeroSlides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ButtonText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroSlides", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "HeroSlides",
                columns: new[] { "Id", "ButtonText", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsActive", "Subtitle", "TargetUrl", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Shop now", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A specialist label creating luxury essentials. Ethically crafted with an unwavering commitment to exceptional quality.", 1, "malefashion/img/hero/hero-1.jpg", true, "Summer Collection", "/Shop", "Fall - Winter Collections 2030", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Shop now", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A specialist label creating luxury essentials. Ethically crafted with an unwavering commitment to exceptional quality.", 2, "malefashion/img/hero/hero-2.jpg", true, "Summer Collection", "/Shop", "Fall - Winter Collections 2030", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeroSlides");
        }
    }
}
