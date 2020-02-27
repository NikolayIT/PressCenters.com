namespace PressCenters.Data.Migrations
{
    using System;

    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class RemoveWorkerTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerTasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkerTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    Parameters = table.Column<string>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    Processed = table.Column<bool>(nullable: false),
                    Processing = table.Column<bool>(nullable: false),
                    ProcessingComment = table.Column<string>(nullable: true),
                    Result = table.Column<string>(nullable: true),
                    RunAfter = table.Column<DateTime>(nullable: true),
                    TypeName = table.Column<string>(maxLength: 100, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerTasks", x => x.Id);
                });
        }
    }
}
