using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailRetryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailAttemptCount",
                table: "NotificationRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmailLastError",
                table: "NotificationRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailPayloadJson",
                table: "NotificationRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAt",
                table: "NotificationRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailStatus",
                table: "NotificationRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextEmailRetryAt",
                table: "NotificationRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecords_EmailRetry",
                table: "NotificationRecords",
                columns: new[] { "EmailStatus", "NextEmailRetryAt" },
                filter: "[EmailStatus] IN (1, 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationRecords_EmailRetry",
                table: "NotificationRecords");

            migrationBuilder.DropColumn(
                name: "EmailAttemptCount",
                table: "NotificationRecords");

            migrationBuilder.DropColumn(
                name: "EmailLastError",
                table: "NotificationRecords");

            migrationBuilder.DropColumn(
                name: "EmailPayloadJson",
                table: "NotificationRecords");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "NotificationRecords");

            migrationBuilder.DropColumn(
                name: "EmailStatus",
                table: "NotificationRecords");

            migrationBuilder.DropColumn(
                name: "NextEmailRetryAt",
                table: "NotificationRecords");
        }
    }
}
