using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeRoleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OrgAdmin shifted from 2 → 3; HRManager is the new 2.
            // Promote existing OrgAdmin rows before the new value takes that slot.
            migrationBuilder.Sql(
                "UPDATE \"Memberships\" SET \"Role\" = 3 WHERE \"Role\" = 2;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Memberships\" SET \"Role\" = 2 WHERE \"Role\" = 3;");
        }
    }
}
