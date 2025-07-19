using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreMigrations.Migrations
{
    /// <inheritdoc />
    public partial class TestPaperManageCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "testPapersManageId",
                table: "testpapers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestPapersManages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExcelFilePath = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileLabel = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuestionBankCourseTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasAnsweringStarted = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CreateUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPapersManages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_testpapers_testPapersManageId",
                table: "testpapers",
                column: "testPapersManageId");

            migrationBuilder.AddForeignKey(
                name: "FK_testpapers_TestPapersManages_testPapersManageId",
                table: "testpapers",
                column: "testPapersManageId",
                principalTable: "TestPapersManages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_testpapers_TestPapersManages_testPapersManageId",
                table: "testpapers");

            migrationBuilder.DropTable(
                name: "TestPapersManages");

            migrationBuilder.DropIndex(
                name: "IX_testpapers_testPapersManageId",
                table: "testpapers");

            migrationBuilder.DropColumn(
                name: "testPapersManageId",
                table: "testpapers");
        }
    }
}
