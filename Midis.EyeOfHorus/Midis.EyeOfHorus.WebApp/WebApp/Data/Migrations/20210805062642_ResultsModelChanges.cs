using Microsoft.EntityFrameworkCore.Migrations;

namespace Midis.EyeOfHorus.WebApp.Migrations
{
    public partial class ResultsModelChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Results",
                newName: "Worker");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Worker",
                table: "Results",
                newName: "FullName");
        }
    }
}
