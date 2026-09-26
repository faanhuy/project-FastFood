using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    size_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity_available = table.Column<int>(type: "integer", nullable: false),
                    quantity_reserved = table.Column<int>(type: "integer", nullable: false),
                    low_stock_threshold = table.Column<int>(type: "integer", nullable: false),
                    // xmin là cột hệ thống của PostgreSQL (có sẵn ở mọi bảng, không tạo được bằng CREATE TABLE).
                    // Model vẫn map nó làm concurrency token (InventoryItemConfiguration); ở đây cố ý KHÔNG tạo cột.
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_items", x => x.id);
                    table.CheckConstraint("ck_inventory_items_quantities", "quantity_available >= 0 AND quantity_reserved >= 0 AND quantity_reserved <= quantity_available");
                });

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_reservations_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_store_id_product_id",
                table: "inventory_items",
                columns: new[] { "store_id", "product_id" },
                unique: true,
                filter: "size_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_store_id_product_id_size_id",
                table: "inventory_items",
                columns: new[] { "store_id", "product_id", "size_id" },
                unique: true,
                filter: "size_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_inventory_item_id",
                table: "stock_reservations",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_order_id_inventory_item_id",
                table: "stock_reservations",
                columns: new[] { "order_id", "inventory_item_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_reservations");

            migrationBuilder.DropTable(
                name: "inventory_items");
        }
    }
}
