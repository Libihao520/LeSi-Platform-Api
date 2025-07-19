using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreMigrations.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseSpecification3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "totalPoints",
                table: "ReportCards",
                newName: "TotalPoints");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "ReportCards",
                newName: "Subject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalPoints",
                table: "ReportCards",
                newName: "totalPoints");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "ReportCards",
                newName: "subject");
        }
    }
}
