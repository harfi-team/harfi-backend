using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harfi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityTokenToEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityToken",
                table: "EmailVerifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityToken",
                table: "EmailVerifications");
        }
    }
}
