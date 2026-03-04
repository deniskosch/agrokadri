using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agrojob.Migrations
{
    /// <inheritdoc />
    public partial class refactor3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Resumes",
                newName: "Patronymic");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Resumes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Resumes");

            migrationBuilder.RenameColumn(
                name: "Patronymic",
                table: "Resumes",
                newName: "FullName");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Resumes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}
