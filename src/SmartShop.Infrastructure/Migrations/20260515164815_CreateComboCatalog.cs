using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateComboCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Combos]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Combos] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [OriginalPrice] decimal(18,2) NOT NULL,
        [SalePrice] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_Combos_IsActive] DEFAULT CAST(1 AS bit),
        [StartsAt] datetime2 NOT NULL,
        [EndsAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Combos] PRIMARY KEY ([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ComboItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ComboItems] (
        [Id] uniqueidentifier NOT NULL,
        [ComboId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [SizeId] uniqueidentifier NULL,
        [SizeLabel] nvarchar(100) NULL,
        [Quantity] int NOT NULL,
        [UnitPriceSnapshot] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ComboItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComboItems_Combos_ComboId] FOREIGN KEY ([ComboId]) REFERENCES [dbo].[Combos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComboItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ComboItems_ComboId'
      AND object_id = OBJECT_ID(N'[dbo].[ComboItems]')
)
BEGIN
    CREATE INDEX [IX_ComboItems_ComboId] ON [dbo].[ComboItems] ([ComboId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ComboItems_ProductId'
      AND object_id = OBJECT_ID(N'[dbo].[ComboItems]')
)
BEGIN
    CREATE INDEX [IX_ComboItems_ProductId] ON [dbo].[ComboItems] ([ProductId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Combo_Active'
      AND object_id = OBJECT_ID(N'[dbo].[Combos]')
)
BEGIN
    CREATE INDEX [IX_Combo_Active] ON [dbo].[Combos] ([IsActive], [StartsAt], [EndsAt]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ComboItems]', N'U') IS NOT NULL
    DROP TABLE [dbo].[ComboItems];
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Combos]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Combos];
");
        }
    }
}
