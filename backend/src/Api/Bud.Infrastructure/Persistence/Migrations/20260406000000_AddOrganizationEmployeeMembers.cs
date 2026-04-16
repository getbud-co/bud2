using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Memberships table
            migrationBuilder.CreateTable(
                name: "Memberships",
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
                    table.PrimaryKey("PK_Memberships", x => new { x.EmployeeId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_Memberships_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Memberships_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Memberships_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Memberships_Employees_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Migrate existing data from Employees to Memberships
            migrationBuilder.Sql(@"
                INSERT INTO ""Memberships"" (""EmployeeId"", ""OrganizationId"", ""Role"", ""TeamId"", ""LeaderId"", ""IsGlobalAdmin"")
                SELECT ""Id"", ""OrganizationId"", ""Role"", ""TeamId"", ""LeaderId"", ""IsGlobalAdmin""
                FROM ""Employees""
                WHERE ""OrganizationId"" IS NOT NULL
            ");

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Memberships_OrganizationId",
                table: "Memberships",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_TeamId",
                table: "Memberships",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_LeaderId",
                table: "Memberships",
                column: "LeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Memberships");
        }
    }
}
