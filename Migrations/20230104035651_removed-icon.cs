using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace newsApi.Migrations
{
    public partial class removedicon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconPath",
                table: "Stories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconPath",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
