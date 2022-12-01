using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace newsApi.Migrations
{
    public partial class createStoryAndImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageClass",
                table: "ImageDbs");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HtmlData",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "HtmlData",
                table: "Stories");

            migrationBuilder.AddColumn<int>(
                name: "ImageClass",
                table: "ImageDbs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
