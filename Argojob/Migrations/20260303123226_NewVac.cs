using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agrojob.Migrations
{
    /// <inheritdoc />
    public partial class NewVac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Vacancies");

            migrationBuilder.AddColumn<long>(
                name: "FixedSalary",
                table: "Vacancies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SalaryFrom",
                table: "Vacancies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SalaryTo",
                table: "Vacancies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalaryType",
                table: "Vacancies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedSalary",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "SalaryFrom",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "SalaryTo",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "SalaryType",
                table: "Vacancies");

            migrationBuilder.AddColumn<string>(
                name: "Salary",
                table: "Vacancies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
