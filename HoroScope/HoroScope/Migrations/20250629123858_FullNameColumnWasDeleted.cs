using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoroScope.Migrations
{
    /// <inheritdoc />
    public partial class FullNameColumnWasDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "BlogComments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "BlogComments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
