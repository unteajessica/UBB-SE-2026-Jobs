using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PussyCats.Library.Migrations
{
    /// <inheritdoc />
    public partial class FixChatSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove messages for chat 1 (had wrong sender IDs — both were SenderId=1)
            migrationBuilder.DeleteData("Messages", "MessageId", 1);
            migrationBuilder.DeleteData("Messages", "MessageId", 2);

            // Switch chat 1 to CompanyId=2 (CloudWorks) so CompanyId(2) != UserId(1)
            // — sender identity is unambiguous when Alice(1) and CloudWorks(2) message each other
            migrationBuilder.Sql("UPDATE Chats SET CompanyId = 2 WHERE ChatId = 1");

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "MessageId", "ChatId", "SenderId", "Content", "Timestamp", "Type", "IsRead", "OriginalFileName" },
                values: new object[,]
                {
                    { 1, 1, 1, "Hi, I saw the open positions at CloudWorks and I'm very interested!", new DateTime(2026, 5, 7, 9, 0, 0, DateTimeKind.Utc), 0, true,  "" },
                    { 2, 1, 2, "We are currently hiring for a Backend Developer role. Would you like to apply?", new DateTime(2026, 5, 7, 9, 5, 0, DateTimeKind.Utc), 0, false, "" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("Messages", "MessageId", 1);
            migrationBuilder.DeleteData("Messages", "MessageId", 2);
            migrationBuilder.Sql("UPDATE Chats SET CompanyId = 1 WHERE ChatId = 1");
            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "MessageId", "ChatId", "SenderId", "Content", "Timestamp", "Type", "IsRead", "OriginalFileName" },
                values: new object[,]
                {
                    { 1, 1, 1, "Hi, I saw the open positions at TechNova and I'm very interested!", new DateTime(2026, 5, 7, 9, 0, 0, DateTimeKind.Utc), 0, true,  "" },
                    { 2, 1, 1, "We are currently hiring for a Backend Developer role. Would you like to apply?", new DateTime(2026, 5, 7, 9, 5, 0, DateTimeKind.Utc), 0, false, "" },
                });
        }
    }
}
