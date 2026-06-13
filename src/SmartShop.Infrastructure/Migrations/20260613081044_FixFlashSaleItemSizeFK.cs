using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixFlashSaleItemSizeFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashSaleItems_Sizes_SizeId",
                table: "FlashSaleItems");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleItems_ProductSizes_SizeId",
                table: "FlashSaleItems",
                column: "SizeId",
                principalTable: "ProductSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashSaleItems_ProductSizes_SizeId",
                table: "FlashSaleItems");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleItems_Sizes_SizeId",
                table: "FlashSaleItems",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
