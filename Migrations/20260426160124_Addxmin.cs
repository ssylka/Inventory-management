using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Managment.Migrations
{
    /// <inheritdoc />
    public partial class Addxmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Items");

            migrationBuilder.AddColumn<long>(
                name: "xmin",
                table: "Items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Items");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Items",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
