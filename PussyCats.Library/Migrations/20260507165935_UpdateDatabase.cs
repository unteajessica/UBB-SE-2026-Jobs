using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PussyCats.Library.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        //    migrationBuilder.InsertData(
        //        table: "Users",
        //        columns: new[] { "UserId", "ActiveAccount", "Address", "Age", "City", "Country", "CreatedAt", "CurrentLevel", "Degree", "Email", "ExpectedGraduationYear", "FirstName", "Gender", "GitHub", "HasDisabilities", "LastName", "LastUpdated", "LinkedIn", "LocationPreference", "Motivation", "ParsedCv", "Phone", "PreferredEmploymentType", "ProfilePicturePath", "TotalExperiencePoints", "University", "UniversityStartYear", "WorkModePreference", "YearsOfExperience" },
        //        values: new object[] { 1, true, "123 Main St", 25, "Bucharest", "Romania", new DateTime(2025, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Computer Science", "alice.smith@example.com", 2022, "Alice", "", "", false, "Smith", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "", "", "", "+40123456789", "", "", 0, "University of Bucharest", 2018, "", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "Users",
            //    keyColumn: "UserId",
            //    keyValue: 1);
        }
    }
}
