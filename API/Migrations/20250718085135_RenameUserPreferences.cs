using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_UserPreferences_UserId",
                table: "Deliveries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences");

            migrationBuilder.RenameTable(
                name: "UserPreferences",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_UserPreferences_UserId",
                table: "Users",
                newName: "IX_Users_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Users_UserId",
                table: "Deliveries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Users_UserId",
                table: "Deliveries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "UserPreferences");

            migrationBuilder.RenameIndex(
                name: "IX_Users_UserId",
                table: "UserPreferences",
                newName: "IX_UserPreferences_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_UserPreferences_UserId",
                table: "Deliveries",
                column: "UserId",
                principalTable: "UserPreferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
