using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationEmployeeMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create OrganizationEmployeeMembers table
            migrationBuilder.CreateTable(
                name: "OrganizationEmployeeMembers",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsGlobalAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationEmployeeMembers", x => new { x.EmployeeId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_OrganizationEmployeeMembers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationEmployeeMembers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationEmployeeMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrganizationEmployeeMembers_Employees_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Migrate existing data from Employees to OrganizationEmployeeMembers
            migrationBuilder.Sql(@"
                INSERT INTO ""OrganizationEmployeeMembers"" (""EmployeeId"", ""OrganizationId"", ""Role"", ""TeamId"", ""LeaderId"", ""IsGlobalAdmin"")
                SELECT ""Id"", ""OrganizationId"", ""Role"", ""TeamId"", ""LeaderId"", ""IsGlobalAdmin""
                FROM ""Employees""
                WHERE ""OrganizationId"" IS NOT NULL
            ");

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_OrganizationEmployeeMembers_OrganizationId",
                table: "OrganizationEmployeeMembers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationEmployeeMembers_TeamId",
                table: "OrganizationEmployeeMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationEmployeeMembers_LeaderId",
                table: "OrganizationEmployeeMembers",
                column: "LeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OrganizationEmployeeMembers");
        }
    }
}
