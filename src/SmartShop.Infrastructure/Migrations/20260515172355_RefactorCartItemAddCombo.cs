using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCartItemAddCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @isNullable bit = (
    SELECT CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CartItems' AND COLUMN_NAME = 'ProductId'
);
IF @isNullable = 0
    ALTER TABLE [dbo].[CartItems] ALTER COLUMN [ProductId] uniqueidentifier NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[CartItems]', N'ComboId') IS NULL
    ALTER TABLE [dbo].[CartItems] ADD [ComboId] uniqueidentifier NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[CartItems]', N'DisplayName') IS NULL
    ALTER TABLE [dbo].[CartItems] ADD [DisplayName] nvarchar(300) NOT NULL DEFAULT '';
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[CartItems]', N'ImageUrl') IS NULL
    ALTER TABLE [dbo].[CartItems] ADD [ImageUrl] nvarchar(500) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[CartItems]', N'ItemType') IS NULL
    ALTER TABLE [dbo].[CartItems] ADD [ItemType] nvarchar(20) NOT NULL DEFAULT '';
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[CartItemComponents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItemComponents] (
        [Id] uniqueidentifier NOT NULL,
        [CartItemId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [SizeId] uniqueidentifier NULL,
        [SizeLabel] nvarchar(100) NULL,
        [QuantityPerCombo] int NOT NULL,
        [TotalQuantity] int NOT NULL,
        [UnitPriceSnapshot] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_CartItemComponents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItemComponents_CartItems_CartItemId] FOREIGN KEY ([CartItemId]) REFERENCES [dbo].[CartItems] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_CartItemComponents_CartItemId] ON [dbo].[CartItemComponents] ([CartItemId]);
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[CartItemComponents]', N'TotalQuantity') IS NULL
    ALTER TABLE [dbo].[CartItemComponents] ADD [TotalQuantity] int NOT NULL DEFAULT 0;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItemComponents");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "CartItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
