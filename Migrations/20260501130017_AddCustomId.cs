using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Format",
                table: "CustomIdElements");

            migrationBuilder.AddColumn<int>(
                name: "LastSequence",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSequence",
                table: "Inventories");

            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "CustomIdElements",
                type: "text",
                nullable: true);
        }
    }
}
