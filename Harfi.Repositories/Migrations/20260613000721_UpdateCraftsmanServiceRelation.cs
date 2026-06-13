using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harfi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCraftsmanServiceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Craftsmen_Users_UserId",
                table: "Craftsmen");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsDeleted",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Craftsmen");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rating",
                table: "Craftsmen",
                type: "decimal(3,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ServiceTypeId",
                table: "Craftsmen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Craftsmen_ServiceTypeId",
                table: "Craftsmen",
                column: "ServiceTypeId");

            migrationBuilder.Sql("UPDATE [Craftsmen] SET [ServiceTypeId] = (SELECT TOP 1 [Id] FROM [ServiceTypes]) WHERE [ServiceTypeId] = 0 OR [ServiceTypeId] IS NULL OR [ServiceTypeId] NOT IN (SELECT [Id] FROM [ServiceTypes])");

            migrationBuilder.AddForeignKey(
                name: "FK_Craftsmen_ServiceTypes_ServiceTypeId",
                table: "Craftsmen",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Craftsmen_Users_UserId",
                table: "Craftsmen",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Craftsmen_ServiceTypes_ServiceTypeId",
                table: "Craftsmen");

            migrationBuilder.DropForeignKey(
                name: "FK_Craftsmen_Users_UserId",
                table: "Craftsmen");

            migrationBuilder.DropIndex(
                name: "IX_Craftsmen_ServiceTypeId",
                table: "Craftsmen");

            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                table: "Craftsmen");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rating",
                table: "Craftsmen",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "Craftsmen",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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
                name: "FK_Craftsmen_Users_UserId",
                table: "Craftsmen",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
