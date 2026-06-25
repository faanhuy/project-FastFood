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

            // Guarded: on a fresh DB, AddComboColumnsToOrders already created this table —
            // only create it here if that didn't happen (the broken-history case this migration fixes).
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComboPromotions')
BEGIN
    CREATE TABLE [ComboPromotions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [TriggerProductId] uniqueidentifier NOT NULL,
        [TriggerSizeId] uniqueidentifier NULL,
        [TriggerMinQuantity] int NOT NULL DEFAULT 1,
        [RewardType] int NOT NULL,
        [RewardProductId] uniqueidentifier NULL,
        [RewardSizeId] uniqueidentifier NULL,
        [RewardQuantity] int NULL,
        [RewardAmount] decimal(18,2) NULL,
        [StoreId] uniqueidentifier NULL,
        [StartsAt] datetime2 NULL,
        [EndsAt] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_ComboPromotions] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_ComboPromotions_IsActive_StartsAt_EndsAt] ON [ComboPromotions] ([IsActive], [StartsAt], [EndsAt]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComboPromotions");
        }
    }
}
