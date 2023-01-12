using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace newsApi.Migrations
{
    public partial class corrections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryTags_Stories_StoryId",
                table: "StoryTags");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTags_Tags_TagName",
                table: "StoryTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StoryTags",
                table: "StoryTags");

            migrationBuilder.RenameTable(
                name: "StoryTags",
                newName: "StoryTag");

            migrationBuilder.RenameIndex(
                name: "IX_StoryTags_TagName",
                table: "StoryTag",
                newName: "IX_StoryTag_TagName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StoryTag",
                table: "StoryTag",
                columns: new[] { "StoryId", "TagName" });

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTag_Stories_StoryId",
                table: "StoryTag",
                column: "StoryId",
                principalTable: "Stories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTag_Tags_TagName",
                table: "StoryTag",
                column: "TagName",
                principalTable: "Tags",
                principalColumn: "TagName",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryTag_Stories_StoryId",
                table: "StoryTag");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTag_Tags_TagName",
                table: "StoryTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StoryTag",
                table: "StoryTag");

            migrationBuilder.RenameTable(
                name: "StoryTag",
                newName: "StoryTags");

            migrationBuilder.RenameIndex(
                name: "IX_StoryTag_TagName",
                table: "StoryTags",
                newName: "IX_StoryTags_TagName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StoryTags",
                table: "StoryTags",
                columns: new[] { "StoryId", "TagName" });

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTags_Stories_StoryId",
                table: "StoryTags",
                column: "StoryId",
                principalTable: "Stories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTags_Tags_TagName",
                table: "StoryTags",
                column: "TagName",
                principalTable: "Tags",
                principalColumn: "TagName",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
