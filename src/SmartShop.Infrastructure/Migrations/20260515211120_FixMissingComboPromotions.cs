using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingComboPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing Orders columns (were skipped when AddComboColumnsToOrders was recorded without running DDL)
            migrationBuilder.Sql(@"
IF COL_LENGTH('Orders', 'ComboDiscountAmount') IS NULL
    ALTER TABLE [Orders] ADD [ComboDiscountAmount] decimal(18,2) NOT NULL DEFAULT 0;
");
            migrationBuilder.Sql(@"
IF COL_LENGTH('Orders', 'ComboPromotionId') IS NULL
    ALTER TABLE [Orders] ADD [ComboPromotionId] uniqueidentifier NULL;
");

            migrationBuilder.CreateTable(
                name: "ComboPromotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TriggerProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriggerSizeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TriggerMinQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RewardType = table.Column<int>(type: "int", nullable: false),
                    RewardProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RewardSizeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RewardQuantity = table.Column<int>(type: "int", nullable: true),
                    RewardAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboPromotions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboPromotions_IsActive_StartsAt_EndsAt",
                table: "ComboPromotions",
                columns: new[] { "IsActive", "StartsAt", "EndsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComboPromotions");
        }
    }
}
