using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOrderItemAddCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @isNullable bit = (
    SELECT CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'ProductId'
);
IF @isNullable = 0
    ALTER TABLE [dbo].[OrderItems] ALTER COLUMN [ProductId] uniqueidentifier NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[OrderItems]', N'ComboId') IS NULL
    ALTER TABLE [dbo].[OrderItems] ADD [ComboId] uniqueidentifier NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[OrderItems]', N'ImageUrl') IS NULL
    ALTER TABLE [dbo].[OrderItems] ADD [ImageUrl] nvarchar(500) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[OrderItems]', N'ItemType') IS NULL
    ALTER TABLE [dbo].[OrderItems] ADD [ItemType] nvarchar(20) NOT NULL DEFAULT '';
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[OrderItemComponents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderItemComponents] (
        [Id] uniqueidentifier NOT NULL,
        [OrderItemId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [ProductImageUrl] nvarchar(500) NULL,
        [SizeId] uniqueidentifier NULL,
        [SizeLabel] nvarchar(100) NULL,
        [QuantityPerCombo] int NOT NULL,
        [TotalQuantity] int NOT NULL,
        [UnitPriceSnapshot] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_OrderItemComponents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItemComponents_OrderItems_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES [dbo].[OrderItems] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_OrderItemComponents_OrderItemId] ON [dbo].[OrderItemComponents] ([OrderItemId]);
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[OrderItemComponents]', N'TotalQuantity') IS NULL
    ALTER TABLE [dbo].[OrderItemComponents] ADD [TotalQuantity] int NOT NULL DEFAULT 0;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[OrderItemComponents]', N'ProductImageUrl') IS NULL
    ALTER TABLE [dbo].[OrderItemComponents] ADD [ProductImageUrl] nvarchar(500) NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemComponents");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "OrderItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
