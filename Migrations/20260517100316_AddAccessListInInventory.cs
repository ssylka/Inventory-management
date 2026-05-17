using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessListInInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "Inventories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
