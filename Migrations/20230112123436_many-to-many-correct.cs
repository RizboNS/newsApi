using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace newsApi.Migrations
{
    public partial class manytomanycorrect : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryTag_Stories_StoriesId",
                table: "StoryTag");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTag_Tags_TagsTagName",
                table: "StoryTag");

            migrationBuilder.RenameColumn(
                name: "TagsTagName",
                table: "StoryTag",
                newName: "TagName");

            migrationBuilder.RenameColumn(
                name: "StoriesId",
                table: "StoryTag",
                newName: "StoryId");

            migrationBuilder.RenameIndex(
                name: "IX_StoryTag_TagsTagName",
                table: "StoryTag",
                newName: "IX_StoryTag_TagName");

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

            migrationBuilder.RenameColumn(
                name: "TagName",
                table: "StoryTag",
                newName: "TagsTagName");

            migrationBuilder.RenameColumn(
                name: "StoryId",
                table: "StoryTag",
                newName: "StoriesId");

            migrationBuilder.RenameIndex(
                name: "IX_StoryTag_TagName",
                table: "StoryTag",
                newName: "IX_StoryTag_TagsTagName");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTag_Stories_StoriesId",
                table: "StoryTag",
                column: "StoriesId",
                principalTable: "Stories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTag_Tags_TagsTagName",
                table: "StoryTag",
                column: "TagsTagName",
                principalTable: "Tags",
                principalColumn: "TagName",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
