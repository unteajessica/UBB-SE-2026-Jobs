using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PussyCats.Library.Migrations
{
    /// <inheritdoc />
    public partial class SeedChatDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "ActiveAccount", "Address", "Age", "City", "Country", "CreatedAt", "CurrentLevel", "Degree", "Email", "ExpectedGraduationYear", "FirstName", "Gender", "GitHub", "HasDisabilities", "LastName", "LastUpdated", "LinkedIn", "LocationPreference", "Motivation", "ParsedCv", "Phone", "PreferredEmploymentType", "ProfilePicturePath", "TotalExperiencePoints", "University", "UniversityStartYear", "WorkModePreference", "YearsOfExperience" },
                values: new object[] { 2, true, "456 Oak Ave", 27, "Cluj-Napoca", "Romania", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Software Engineering", "bob.jones@example.com", 2021, "Bob", "", "", false, "Jones", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "", "", "", "+40123456780", "", "", 0, "Babes-Bolyai University", 2017, "", 2 });

            migrationBuilder.InsertData(
                table: "Chats",
                columns: new[] { "ChatId", "UserId", "CompanyId", "SecondUserId", "JobId", "IsBlocked", "BlockedByUserId", "DeletedAtByUser", "DeletedAtBySecondParty" },
                values: new object[,]
                {
                    { 1, 1, 1, null, null, false, null, null, null },
                    { 2, 1, null, 2, null, false, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "MessageId", "ChatId", "SenderId", "Content", "Timestamp", "Type", "IsRead", "OriginalFileName" },
                values: new object[,]
                {
                    { 1, 1, 1,  "Hi, I saw the open positions at TechNova and I'm very interested!", new DateTime(2026, 5, 7, 9, 0, 0, DateTimeKind.Utc), 0, true,  "" },
                    { 2, 1, 1,  "We are currently hiring for a Backend Developer role. Would you like to apply?", new DateTime(2026, 5, 7, 9, 5, 0, DateTimeKind.Utc), 0, false, "" },
                    { 3, 2, 2,  "Hey Alice! Long time no see, how's the job hunt going?", new DateTime(2026, 5, 7, 10, 0, 0, DateTimeKind.Utc), 0, true,  "" },
                    { 4, 2, 1,  "Hi Bob! It's going well, I have a chat with TechNova actually.", new DateTime(2026, 5, 7, 10, 3, 0, DateTimeKind.Utc), 0, false, "" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("Messages", "MessageId", new object[] { 1, 2, 3, 4 });
            migrationBuilder.DeleteData("Chats", "ChatId", new object[] { 1, 2 });
            migrationBuilder.DeleteData("Users", "UserId", 2);
        }
    }
}
