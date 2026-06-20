using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harfi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class updatenotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ConversationId",
                table: "Notifications",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Conversations_ConversationId",
                table: "Notifications",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Conversations_ConversationId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ConversationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Notifications");
        }
    }
}
