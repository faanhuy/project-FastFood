using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartShop.Infrastructure.Data;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260516120000_FixCartItemTypeToNvarchar")]
    public partial class FixCartItemTypeToNvarchar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop default constraint on CartItems.ItemType before altering column type
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CartItems' AND COLUMN_NAME = 'ItemType' AND DATA_TYPE LIKE 'nvarchar%'
)
BEGIN
    DECLARE @constraint_name NVARCHAR(256);
    SELECT @constraint_name = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'CartItems' AND c.name = 'ItemType';

    IF @constraint_name IS NOT NULL
        EXEC('ALTER TABLE [dbo].[CartItems] DROP CONSTRAINT [' + @constraint_name + ']');

    UPDATE [dbo].[CartItems] SET [ItemType] = '1' WHERE [ItemType] = 'Combo';
    UPDATE [dbo].[CartItems] SET [ItemType] = '0' WHERE [ItemType] IN ('Product', '');
    ALTER TABLE [dbo].[CartItems] ALTER COLUMN [ItemType] int NOT NULL;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'ItemType' AND DATA_TYPE LIKE 'nvarchar%'
)
BEGIN
    DECLARE @constraint_name NVARCHAR(256);
    SELECT @constraint_name = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'OrderItems' AND c.name = 'ItemType';

    IF @constraint_name IS NOT NULL
        EXEC('ALTER TABLE [dbo].[OrderItems] DROP CONSTRAINT [' + @constraint_name + ']');

    UPDATE [dbo].[OrderItems] SET [ItemType] = '1' WHERE [ItemType] = 'Combo';
    UPDATE [dbo].[OrderItems] SET [ItemType] = '0' WHERE [ItemType] IN ('Product', '');
    ALTER TABLE [dbo].[OrderItems] ALTER COLUMN [ItemType] int NOT NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CartItems' AND COLUMN_NAME = 'ItemType' AND DATA_TYPE = 'int'
)
BEGIN
    ALTER TABLE [dbo].[CartItems] ALTER COLUMN [ItemType] nvarchar(20) NOT NULL;
    UPDATE [dbo].[CartItems] SET [ItemType] = 'Combo' WHERE [ItemType] = 1;
    UPDATE [dbo].[CartItems] SET [ItemType] = 'Product' WHERE [ItemType] = 0;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'ItemType' AND DATA_TYPE = 'int'
)
BEGIN
    ALTER TABLE [dbo].[OrderItems] ALTER COLUMN [ItemType] nvarchar(20) NOT NULL;
    UPDATE [dbo].[OrderItems] SET [ItemType] = 'Combo' WHERE [ItemType] = 1;
    UPDATE [dbo].[OrderItems] SET [ItemType] = 'Product' WHERE [ItemType] = 0;
END
");
        }
    }
}
