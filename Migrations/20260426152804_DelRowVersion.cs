using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Managment.Migrations
{
    /// <inheritdoc />
    public partial class DelRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Items");

            migrationBuilder.AddColumn<bool>(
                name: "IsTaken",
                table: "Items",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTaken",
                table: "Items");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Items",
                type: "bytea",
                rowVersion: true,
                nullable: true);
        }
    }
}
