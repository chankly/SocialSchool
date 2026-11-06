using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HSoft.SocialSchool.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSportsCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Sports",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-4000-8000-000000000001"), true, "Skate" },
                    { new Guid("a1b2c3d4-0002-4000-8000-000000000002"), true, "Surf" },
                    { new Guid("a1b2c3d4-0003-4000-8000-000000000003"), true, "Snowboard" },
                    { new Guid("a1b2c3d4-0004-4000-8000-000000000004"), true, "BMX" },
                    { new Guid("a1b2c3d4-0005-4000-8000-000000000005"), true, "Parkour" },
                    { new Guid("a1b2c3d4-0006-4000-8000-000000000006"), true, "Escalada" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sports",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0001-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Sports",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0002-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Sports",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0003-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Sports",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0004-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Sports",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0005-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Sports",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0006-4000-8000-000000000006"));
        }
    }
}
