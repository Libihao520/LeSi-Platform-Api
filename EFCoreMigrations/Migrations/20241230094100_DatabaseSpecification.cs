using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreMigrations.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_testpapers_TestPapersManages_testPapersManageId",
                table: "testpapers");

            migrationBuilder.DropForeignKey(
                name: "FK_yolotbs_Photos_PhotosId",
                table: "yolotbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_yolotbs",
                table: "yolotbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_testpapers",
                table: "testpapers");

            migrationBuilder.RenameTable(
                name: "yolotbs",
                newName: "YoloTbs");

            migrationBuilder.RenameTable(
                name: "testpapers",
                newName: "TestPapers");

            migrationBuilder.RenameIndex(
                name: "IX_yolotbs_PhotosId",
                table: "YoloTbs",
                newName: "IX_YoloTbs_PhotosId");

            migrationBuilder.RenameIndex(
                name: "IX_testpapers_testPapersManageId",
                table: "TestPapers",
                newName: "IX_TestPapers_testPapersManageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YoloTbs",
                table: "YoloTbs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestPapers",
                table: "TestPapers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestPapers_TestPapersManages_testPapersManageId",
                table: "TestPapers",
                column: "testPapersManageId",
                principalTable: "TestPapersManages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YoloTbs_Photos_PhotosId",
                table: "YoloTbs",
                column: "PhotosId",
                principalTable: "Photos",
                principalColumn: "PhotosId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestPapers_TestPapersManages_testPapersManageId",
                table: "TestPapers");

            migrationBuilder.DropForeignKey(
                name: "FK_YoloTbs_Photos_PhotosId",
                table: "YoloTbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YoloTbs",
                table: "YoloTbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestPapers",
                table: "TestPapers");

            migrationBuilder.RenameTable(
                name: "YoloTbs",
                newName: "yolotbs");

            migrationBuilder.RenameTable(
                name: "TestPapers",
                newName: "testpapers");

            migrationBuilder.RenameIndex(
                name: "IX_YoloTbs_PhotosId",
                table: "yolotbs",
                newName: "IX_yolotbs_PhotosId");

            migrationBuilder.RenameIndex(
                name: "IX_TestPapers_testPapersManageId",
                table: "testpapers",
                newName: "IX_testpapers_testPapersManageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_yolotbs",
                table: "yolotbs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_testpapers",
                table: "testpapers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_testpapers_TestPapersManages_testPapersManageId",
                table: "testpapers",
                column: "testPapersManageId",
                principalTable: "TestPapersManages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_yolotbs_Photos_PhotosId",
                table: "yolotbs",
                column: "PhotosId",
                principalTable: "Photos",
                principalColumn: "PhotosId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
