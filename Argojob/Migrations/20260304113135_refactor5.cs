using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agrojob.Migrations
{
    /// <inheritdoc />
    public partial class refactor5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Skills",
                table: "Resumes",
                type: "nvarchar(max)",
                maxLength: 100000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Experience",
                table: "Resumes",
                type: "nvarchar(max)",
                maxLength: 100000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Education",
                table: "Resumes",
                type: "nvarchar(max)",
                maxLength: 100000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "About",
                table: "Resumes",
                type: "nvarchar(max)",
                maxLength: 100000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Skills",
                table: "Resumes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 100000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Experience",
                table: "Resumes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 100000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Education",
                table: "Resumes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 100000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "About",
                table: "Resumes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 100000,
                oldNullable: true);
        }
    }
}
