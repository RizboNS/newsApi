using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace newsApi.Migrations
{
    public partial class imageDbremovelocationdomainfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationDomain",
                table: "ImageDbs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationDomain",
                table: "ImageDbs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
