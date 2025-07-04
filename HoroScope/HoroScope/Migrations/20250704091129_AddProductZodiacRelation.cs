using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoroScope.Migrations
{
    /// <inheritdoc />
    public partial class AddProductZodiacRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductZodiacs",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ZodiacId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductZodiacs", x => new { x.ProductId, x.ZodiacId });
                    table.ForeignKey(
                        name: "FK_ProductZodiacs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductZodiacs_Zodiacs_ZodiacId",
                        column: x => x.ZodiacId,
                        principalTable: "Zodiacs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductZodiacs_ZodiacId",
                table: "ProductZodiacs",
                column: "ZodiacId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductZodiacs");
        }
    }
}
