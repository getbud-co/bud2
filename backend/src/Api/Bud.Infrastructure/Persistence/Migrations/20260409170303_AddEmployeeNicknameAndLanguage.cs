using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeNicknameAndLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Employees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "Employees");
        }
    }
}
