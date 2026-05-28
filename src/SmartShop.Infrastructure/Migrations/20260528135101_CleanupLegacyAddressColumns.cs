using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupLegacyAddressColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceCampaignItems_PriceLists_CampaignId",
                table: "PriceCampaignItems");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_SessionId",
                table: "ChatSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriceLists",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "City",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "District",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StockReceiptItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockReceiptItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StockReceiptItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StockReceiptItems");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "ChatSessions");

            migrationBuilder.RenameTable(
                name: "PriceLists",
                newName: "PriceCampaigns");

            migrationBuilder.RenameIndex(
                name: "IX_PriceLists_StartsAt",
                table: "PriceCampaigns",
                newName: "IX_PriceCampaigns_StartsAt");

            migrationBuilder.RenameIndex(
                name: "IX_PriceLists_EndsAt_IsActive",
                table: "PriceCampaigns",
                newName: "IX_PriceCampaigns_EndsAt_IsActive");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriceCampaigns",
                table: "PriceCampaigns",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceCampaignItems_PriceCampaigns_CampaignId",
                table: "PriceCampaignItems",
                column: "CampaignId",
                principalTable: "PriceCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceCampaignItems_PriceCampaigns_CampaignId",
                table: "PriceCampaignItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriceCampaigns",
                table: "PriceCampaigns");

            migrationBuilder.RenameTable(
                name: "PriceCampaigns",
                newName: "PriceLists");

            migrationBuilder.RenameIndex(
                name: "IX_PriceCampaigns_StartsAt",
                table: "PriceLists",
                newName: "IX_PriceLists_StartsAt");

            migrationBuilder.RenameIndex(
                name: "IX_PriceCampaigns_EndsAt_IsActive",
                table: "PriceLists",
                newName: "IX_PriceLists_EndsAt_IsActive");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "UserAddresses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "UserAddresses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "UserAddresses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StockReceiptItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "StockReceiptItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StockReceiptItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "StockReceiptItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "ChatSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriceLists",
                table: "PriceLists",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SessionId",
                table: "ChatSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceCampaignItems_PriceLists_CampaignId",
                table: "PriceCampaignItems",
                column: "CampaignId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
