using Microsoft.EntityFrameworkCore.Migrations;

namespace Midis.EyeOfHorus.WebApp.Migrations
{
    public partial class WorkerImageCorrection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "Workers");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "Workers",
                newName: "Avatar");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Avatar",
                table: "Workers",
                newName: "ImageData");

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
