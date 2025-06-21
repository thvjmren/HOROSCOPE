using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoroScope.Migrations
{
    /// <inheritdoc />
    public partial class ProductCategoryIdToFeatureWasAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                table: "Features",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Features_ProductCategoryId",
                table: "Features",
                column: "ProductCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Features_ProductCategories_ProductCategoryId",
                table: "Features",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Features_ProductCategories_ProductCategoryId",
                table: "Features");

            migrationBuilder.DropIndex(
                name: "IX_Features_ProductCategoryId",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Features");
        }
    }
}
