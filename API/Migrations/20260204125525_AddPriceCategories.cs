using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prices",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "DefaultPriceCategoryId",
                table: "Deliveries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PriceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceCategories", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_PriceCategories_DefaultPriceCategoryId",
                table: "Deliveries");

            migrationBuilder.DropTable(
                name: "PriceCategories");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_DefaultPriceCategoryId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "Prices",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DefaultPriceCategoryId",
                table: "Deliveries");
        }
    }
}
