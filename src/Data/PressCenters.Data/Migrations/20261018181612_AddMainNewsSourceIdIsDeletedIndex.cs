#nullable disable

namespace PressCenters.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddMainNewsSourceIdIsDeletedIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MainNews_SourceId",
                table: "MainNews");

            migrationBuilder.CreateIndex(
                name: "IX_MainNews_SourceId_IsDeleted",
                table: "MainNews",
                columns: new[] { "SourceId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MainNews_SourceId_IsDeleted",
                table: "MainNews");

            migrationBuilder.CreateIndex(
                name: "IX_MainNews_SourceId",
                table: "MainNews",
                column: "SourceId");
        }
    }
}
