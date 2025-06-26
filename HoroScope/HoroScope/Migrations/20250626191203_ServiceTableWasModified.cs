using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoroScope.Migrations
{
    /// <inheritdoc />
    public partial class ServiceTableWasModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFree",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFree",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Services");
        }
    }
}
