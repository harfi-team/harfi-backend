using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harfi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerifications_Users_UserId",
                table: "EmailVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerifications_Users_UserId",
                table: "EmailVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerifications_Users_UserId",
                table: "EmailVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsDeleted",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerifications_Users_UserId",
                table: "EmailVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
