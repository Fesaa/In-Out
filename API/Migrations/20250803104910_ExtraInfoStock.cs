using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class ExtraInfoStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductStock_ProductId",
                table: "ProductStock");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StockHistory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityAfter",
                table: "StockHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityBefore",
                table: "StockHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "StockHistory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStock_ProductId",
                table: "ProductStock",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductStock_ProductId",
                table: "ProductStock");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StockHistory");

            migrationBuilder.DropColumn(
                name: "QuantityAfter",
                table: "StockHistory");

            migrationBuilder.DropColumn(
                name: "QuantityBefore",
                table: "StockHistory");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "StockHistory");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStock_ProductId",
                table: "ProductStock",
                column: "ProductId");
        }
    }
}
