using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Midis.EyeOfHorus.WebApp.Migrations
{
    public partial class AddTwoFieldsToIdentityUserClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {         

            migrationBuilder.AddColumn<string>(
                name: "ClientID",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "AspNetUsers");          
        }
    }
}
