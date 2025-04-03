using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softwarekueche.Web.Migrations
{
    /// <inheritdoc />
    public partial class PostsPreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Posts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MdPreview",
                table: "Posts",
                type: "nvarchar(max)",
                maxLength: 4096,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Posts SET MdPreview = MdContent");
            migrationBuilder.Sql("UPDATE Posts SET IsPublished = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "MdPreview",
                table: "Posts");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);
        }
    }
}
