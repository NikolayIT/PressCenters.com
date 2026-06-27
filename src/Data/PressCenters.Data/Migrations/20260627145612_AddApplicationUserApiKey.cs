using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PressCenters.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "AspNetUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ApiKey",
                table: "AspNetUsers",
                column: "ApiKey",
                unique: true,
                filter: "[ApiKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ApiKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "AspNetUsers");
        }
    }
}
