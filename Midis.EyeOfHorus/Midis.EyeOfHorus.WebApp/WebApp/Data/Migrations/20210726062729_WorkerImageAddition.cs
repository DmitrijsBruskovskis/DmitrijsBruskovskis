using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Midis.EyeOfHorus.WebApp.Migrations
{
    public partial class WorkerImageAddition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Workers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "Workers");
        }
    }
}
