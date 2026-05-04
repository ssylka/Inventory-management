using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Managment.Migrations
{
    /// <inheritdoc />
    public partial class EditItemEntiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link1",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link2",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link3",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Inventories",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link1",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Link2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Link3",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Inventories");
        }
    }
}
