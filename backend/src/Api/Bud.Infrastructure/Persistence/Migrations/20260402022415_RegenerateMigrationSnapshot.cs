using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RegenerateMigrationSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: SyncSnapshot2 (20260401145251) already applied the equivalent
            // structural rename (Collaborators→Employees, Goals→Missions, etc.).
            // This migration record exists only to maintain history continuity.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: paired with the empty Up().
        }
    }
}
