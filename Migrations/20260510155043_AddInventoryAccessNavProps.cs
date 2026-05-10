using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAccessNavProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_CreatorId",
                table: "Inventories");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Inventories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccess_InventoryId",
                table: "InventoryAccess",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccess_UserId",
                table: "InventoryAccess",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_CreatorId",
                table: "Inventories",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAccess_AspNetUsers_UserId",
                table: "InventoryAccess",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAccess_Inventories_InventoryId",
                table: "InventoryAccess",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_CreatorId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAccess_AspNetUsers_UserId",
                table: "InventoryAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAccess_Inventories_InventoryId",
                table: "InventoryAccess");

            migrationBuilder.DropIndex(
                name: "IX_InventoryAccess_InventoryId",
                table: "InventoryAccess");

            migrationBuilder.DropIndex(
                name: "IX_InventoryAccess_UserId",
                table: "InventoryAccess");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Inventories",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_CreatorId",
                table: "Inventories",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
