using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreMigrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YoloTbs_Photos_PhotosId",
                table: "YoloTbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YoloTbs",
                table: "YoloTbs");

            migrationBuilder.RenameTable(
                name: "YoloTbs",
                newName: "YoLoTbs");

            migrationBuilder.RenameIndex(
                name: "IX_YoloTbs_PhotosId",
                table: "YoLoTbs",
                newName: "IX_YoLoTbs_PhotosId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YoLoTbs",
                table: "YoLoTbs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniqueName",
                table: "Users",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_YoLoTbs_Photos_PhotosId",
                table: "YoLoTbs",
                column: "PhotosId",
                principalTable: "Photos",
                principalColumn: "PhotosId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YoLoTbs_Photos_PhotosId",
                table: "YoLoTbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YoLoTbs",
                table: "YoLoTbs");

            migrationBuilder.DropIndex(
                name: "IX_Users_UniqueName",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "YoLoTbs",
                newName: "YoloTbs");

            migrationBuilder.RenameIndex(
                name: "IX_YoLoTbs_PhotosId",
                table: "YoloTbs",
                newName: "IX_YoloTbs_PhotosId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YoloTbs",
                table: "YoloTbs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YoloTbs_Photos_PhotosId",
                table: "YoloTbs",
                column: "PhotosId",
                principalTable: "Photos",
                principalColumn: "PhotosId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
