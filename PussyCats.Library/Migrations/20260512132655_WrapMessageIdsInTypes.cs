using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PussyCats.Library.Migrations
{
    /// <inheritdoc />
    public partial class WrapMessageIdsInTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chats_BlockedByUserId",
                table: "Chats",
                column: "BlockedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_BlockedByUserId",
                table: "Chats",
                column: "BlockedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_BlockedByUserId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_BlockedByUserId",
                table: "Chats");
        }
    }
}
