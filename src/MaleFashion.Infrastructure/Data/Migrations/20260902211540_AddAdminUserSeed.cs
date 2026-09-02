using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaleFashion.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 0, "F1E852D4-90C3-481B-8526-7B1A45C9E231", "admin@malefashion.com", true, "Admin", "User", false, null, "ADMIN@MALEFASHION.COM", "ADMIN@MALEFASHION.COM", "AQAAAAIAAYagAAAAECmUYOQC6clq1vmykb2nWj9M8My5j1tazee0UFLsxNtr5Hd0lfwAuXxtv8SJBa2cfA==", null, false, "A7C934E2-55B1-4A89-B817-2D9981E3F1A0", false, "admin@malefashion.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
