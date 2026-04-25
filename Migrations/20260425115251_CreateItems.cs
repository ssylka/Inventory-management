using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventory_Managment.Migrations
{
    /// <inheritdoc />
    public partial class CreateItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    CustomId = table.Column<string>(type: "text", nullable: false),
                    String1 = table.Column<string>(type: "text", nullable: true),
                    String2 = table.Column<string>(type: "text", nullable: true),
                    String3 = table.Column<string>(type: "text", nullable: true),
                    Text1 = table.Column<string>(type: "text", nullable: true),
                    Text2 = table.Column<string>(type: "text", nullable: true),
                    Text3 = table.Column<string>(type: "text", nullable: true),
                    Number1 = table.Column<int>(type: "integer", nullable: true),
                    Number2 = table.Column<int>(type: "integer", nullable: true),
                    Number3 = table.Column<int>(type: "integer", nullable: true),
                    Bool1 = table.Column<bool>(type: "boolean", nullable: true),
                    Bool2 = table.Column<bool>(type: "boolean", nullable: true),
                    Bool3 = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryId",
                table: "Items",
                column: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
