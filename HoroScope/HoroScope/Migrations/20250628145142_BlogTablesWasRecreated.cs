using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoroScope.Migrations
{
    /// <inheritdoc />
    public partial class BlogTablesWasRecreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Blogs");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Blogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AppUserId",
                table: "Blogs",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_AspNetUsers_AppUserId",
                table: "Blogs",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_AspNetUsers_AppUserId",
                table: "Blogs");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_AppUserId",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Blogs");

            migrationBuilder.AddColumn<int>(
                name: "CommentsCount",
                table: "Blogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
