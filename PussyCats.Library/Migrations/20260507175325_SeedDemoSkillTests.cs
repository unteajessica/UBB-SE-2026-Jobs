using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PussyCats.Library.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoSkillTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SkillTests",
                columns: new[] { "SkillTestId", "AchievedDate", "Name", "Score", "UserId" },
                values: new object[,]
                {
                    { 1, new DateOnly(2026, 1, 7), "C# Fundamentals", 82, 1 },
                    { 2, new DateOnly(2026, 1, 7), "SQL Server", 76, 1 },
                    { 3, new DateOnly(2026, 1, 7), "Software Design", 88, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SkillTests",
                keyColumn: "SkillTestId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SkillTests",
                keyColumn: "SkillTestId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SkillTests",
                keyColumn: "SkillTestId",
                keyValue: 3);
        }
    }
}
