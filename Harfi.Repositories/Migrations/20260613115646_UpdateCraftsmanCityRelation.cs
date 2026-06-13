using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harfi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCraftsmanCityRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Craftsmen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE c
                SET c.CityId = ct.Id
                FROM Craftsmen c
                INNER JOIN Cities ct ON ct.NameAr = c.City
                WHERE c.City IS NOT NULL AND c.City <> ''
            ");

            migrationBuilder.Sql(@"
                UPDATE c
                SET c.CityId = ct.Id
                FROM Craftsmen c
                CROSS APPLY (SELECT TOP 1 Id FROM Cities ORDER BY Id) ct
                WHERE c.CityId = 0
            ");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Craftsmen");

            migrationBuilder.CreateIndex(
                name: "IX_Craftsmen_CityId",
                table: "Craftsmen",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Craftsmen_Cities_CityId",
                table: "Craftsmen",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Craftsmen_Cities_CityId",
                table: "Craftsmen");

            migrationBuilder.DropIndex(
                name: "IX_Craftsmen_CityId",
                table: "Craftsmen");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Craftsmen",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE c
                SET c.City = ct.NameAr
                FROM Craftsmen c
                INNER JOIN Cities ct ON ct.Id = c.CityId
                WHERE c.CityId <> 0
            ");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Craftsmen");
        }
    }
}
