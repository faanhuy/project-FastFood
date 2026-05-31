using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenHashToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "Users");
        }
    }
}
