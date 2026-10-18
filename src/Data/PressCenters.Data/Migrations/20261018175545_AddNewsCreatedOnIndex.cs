#nullable disable

namespace PressCenters.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddNewsCreatedOnIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Production got this index by hand (18.10.2026) before the model knew about it, so create it only where it is missing.
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[News]') AND name = N'IX_News_IsDeleted_CreatedOn_Id')
    CREATE INDEX [IX_News_IsDeleted_CreatedOn_Id] ON [News] ([IsDeleted], [CreatedOn] DESC, [Id] DESC);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_News_IsDeleted_CreatedOn_Id",
                table: "News");
        }
    }
}
