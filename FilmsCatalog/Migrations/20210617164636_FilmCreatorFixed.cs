using Microsoft.EntityFrameworkCore.Migrations;

namespace FilmsCatalog.Migrations
{
    public partial class FilmCreatorFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Films");

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Films",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Films");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Films",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
