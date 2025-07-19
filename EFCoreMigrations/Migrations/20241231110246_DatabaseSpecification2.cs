using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreMigrations.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseSpecification2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "zql",
                table: "YoloTbs",
                newName: "Zql");

            migrationBuilder.RenameColumn(
                name: "zhl",
                table: "YoloTbs",
                newName: "Zhl");

            migrationBuilder.RenameColumn(
                name: "sbzqCount",
                table: "YoloTbs",
                newName: "SbZqCount");

            migrationBuilder.RenameColumn(
                name: "sbjgCount",
                table: "YoloTbs",
                newName: "SbJgCount");

            migrationBuilder.RenameColumn(
                name: "rgmsCount",
                table: "YoloTbs",
                newName: "RgMsCount");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "PassWord");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "TestPapers",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "Photobase64",
                table: "Photos",
                newName: "PhotoBase64");

            migrationBuilder.RenameColumn(
                name: "ModleCls",
                table: "AiModels",
                newName: "ModelCls");

            migrationBuilder.RenameColumn(
                name: "ModelSizee",
                table: "AiModels",
                newName: "ModelSize");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "TestPapers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "TestPapers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PhotoBase64",
                table: "Photos",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Zql",
                table: "YoloTbs",
                newName: "zql");

            migrationBuilder.RenameColumn(
                name: "Zhl",
                table: "YoloTbs",
                newName: "zhl");

            migrationBuilder.RenameColumn(
                name: "SbZqCount",
                table: "YoloTbs",
                newName: "sbzqCount");

            migrationBuilder.RenameColumn(
                name: "SbJgCount",
                table: "YoloTbs",
                newName: "sbjgCount");

            migrationBuilder.RenameColumn(
                name: "RgMsCount",
                table: "YoloTbs",
                newName: "rgmsCount");

            migrationBuilder.RenameColumn(
                name: "PassWord",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "TestPapers",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "PhotoBase64",
                table: "Photos",
                newName: "Photobase64");

            migrationBuilder.RenameColumn(
                name: "ModelSize",
                table: "AiModels",
                newName: "ModelSizee");

            migrationBuilder.RenameColumn(
                name: "ModelCls",
                table: "AiModels",
                newName: "ModleCls");

            migrationBuilder.UpdateData(
                table: "TestPapers",
                keyColumn: "Topic",
                keyValue: null,
                column: "Topic",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "TestPapers",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "TestPapers",
                keyColumn: "subject",
                keyValue: null,
                column: "subject",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "subject",
                table: "TestPapers",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Photos",
                keyColumn: "Photobase64",
                keyValue: null,
                column: "Photobase64",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Photobase64",
                table: "Photos",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
