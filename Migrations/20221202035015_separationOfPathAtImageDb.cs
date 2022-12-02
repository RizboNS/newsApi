using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace newsApi.Migrations
{
    public partial class separationOfPathAtImageDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "ImageDbs",
                newName: "LocationPath");

            migrationBuilder.AddColumn<string>(
                name: "LocationDomain",
                table: "ImageDbs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationDomain",
                table: "ImageDbs");

            migrationBuilder.RenameColumn(
                name: "LocationPath",
                table: "ImageDbs",
                newName: "Location");
        }
    }
}
