using Microsoft.EntityFrameworkCore.Migrations;

namespace Midis.EyeOfHorus.WebApp.Migrations
{
    public partial class FieldClientIDToWorkersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientID",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientID",
                table: "Workers");
        }
    }
}
