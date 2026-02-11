using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class SetupPriceCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_PriceCategories_DefaultPriceCategoryId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_DefaultPriceCategoryId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "DefaultPriceCategoryId",
                table: "Deliveries");

            migrationBuilder.AddColumn<int>(
                name: "PriceCategoryId",
                table: "Deliveries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultPriceCategoryId",
                table: "Clients",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_PriceCategoryId",
                table: "Deliveries",
                column: "PriceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DefaultPriceCategoryId",
                table: "Clients",
                column: "DefaultPriceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_PriceCategories_DefaultPriceCategoryId",
                table: "Clients",
                column: "DefaultPriceCategoryId",
                principalTable: "PriceCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_PriceCategories_PriceCategoryId",
                table: "Deliveries",
                column: "PriceCategoryId",
                principalTable: "PriceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_PriceCategories_DefaultPriceCategoryId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_PriceCategories_PriceCategoryId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_PriceCategoryId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DefaultPriceCategoryId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PriceCategoryId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "DefaultPriceCategoryId",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "DefaultPriceCategoryId",
                table: "Deliveries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DefaultPriceCategoryId",
                table: "Deliveries",
                column: "DefaultPriceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_PriceCategories_DefaultPriceCategoryId",
                table: "Deliveries",
                column: "DefaultPriceCategoryId",
                principalTable: "PriceCategories",
                principalColumn: "Id");
        }
    }
}
