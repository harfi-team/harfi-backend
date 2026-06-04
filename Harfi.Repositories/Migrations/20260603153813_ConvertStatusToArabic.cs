using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harfi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStatusToArabic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert existing status values to Arabic
            migrationBuilder.Sql("UPDATE Jobs SET Status = N'مفتوح' WHERE Status = 'open'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = N'قيد التنفيذ' WHERE Status = 'in-progress'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = N'مكتمل' WHERE Status = 'done'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = N'مرفوض' WHERE Status = 'rejected'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = N'ملغى' WHERE Status = 'cancelled'");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "مفتوح",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "open");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert back to English
            migrationBuilder.Sql("UPDATE Jobs SET Status = 'open' WHERE Status = N'مفتوح'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = 'in-progress' WHERE Status = N'قيد التنفيذ'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = 'done' WHERE Status = N'مكتمل'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = 'rejected' WHERE Status = N'مرفوض'");
            migrationBuilder.Sql("UPDATE Jobs SET Status = 'cancelled' WHERE Status = N'ملغى'");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "open",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "مفتوح");
        }
    }
}
