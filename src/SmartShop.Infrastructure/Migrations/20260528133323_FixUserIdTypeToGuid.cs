using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdTypeToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            // WishlistItems: drop dependent index, convert string → Guid, recreate index
            migrationBuilder.DropIndex(name: "IX_WishlistItems_UserId_ProductId", table: "WishlistItems");
            migrationBuilder.Sql("ALTER TABLE WishlistItems ADD UserIdNew UNIQUEIDENTIFIER NULL;");
            migrationBuilder.Sql("UPDATE WishlistItems SET UserIdNew = TRY_CONVERT(UNIQUEIDENTIFIER, UserId);");
            migrationBuilder.Sql("DELETE FROM WishlistItems WHERE UserIdNew IS NULL;");
            migrationBuilder.Sql("ALTER TABLE WishlistItems DROP COLUMN UserId;");
            migrationBuilder.Sql("EXEC sp_rename 'WishlistItems.UserIdNew', 'UserId', 'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE WishlistItems ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;");
            migrationBuilder.CreateIndex(name: "IX_WishlistItems_UserId_ProductId", table: "WishlistItems", columns: new[] { "UserId", "ProductId" }, unique: true);

            // UserAddresses: drop dependent index, convert string → Guid
            migrationBuilder.DropIndex(name: "IX_UserAddresses_UserId", table: "UserAddresses");
            migrationBuilder.Sql("ALTER TABLE UserAddresses ADD UserIdNew UNIQUEIDENTIFIER NULL;");
            migrationBuilder.Sql("UPDATE UserAddresses SET UserIdNew = TRY_CONVERT(UNIQUEIDENTIFIER, UserId);");
            migrationBuilder.Sql("DELETE FROM UserAddresses WHERE UserIdNew IS NULL;");
            migrationBuilder.Sql("ALTER TABLE UserAddresses DROP COLUMN UserId;");
            migrationBuilder.Sql("EXEC sp_rename 'UserAddresses.UserIdNew', 'UserId', 'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE UserAddresses ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "VnpayTransactionId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Notifications: safe string → Guid conversion
            migrationBuilder.Sql("ALTER TABLE Notifications ADD UserIdNew UNIQUEIDENTIFIER NULL;");
            migrationBuilder.Sql("UPDATE Notifications SET UserIdNew = TRY_CONVERT(UNIQUEIDENTIFIER, UserId);");
            migrationBuilder.Sql("DELETE FROM Notifications WHERE UserIdNew IS NULL;");
            migrationBuilder.Sql("ALTER TABLE Notifications DROP COLUMN UserId;");
            migrationBuilder.Sql("EXEC sp_rename 'Notifications.UserIdNew', 'UserId', 'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE Notifications ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Coupons",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "ChatMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_IsApproved",
                table: "Reviews",
                columns: new[] { "ProductId", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsActive",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VnpayTransactionId",
                table: "Orders",
                column: "VnpayTransactionId",
                unique: true,
                filter: "[VnpayTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddresses_Users_UserId",
                table: "UserAddresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_Users_UserId",
                table: "WishlistItems",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAddresses_Users_UserId",
                table: "UserAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_Users_UserId",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId_IsApproved",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_VnpayTransactionId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Coupons");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "WishlistItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAddresses",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "VnpayTransactionId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Notifications",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "ChatMessages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }
    }
}
