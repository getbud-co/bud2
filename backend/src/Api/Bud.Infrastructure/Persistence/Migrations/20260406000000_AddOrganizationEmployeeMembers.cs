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
            // No-op: UpdateEmployeeSchema (20260407133029) creates the Memberships table
            // and removes old Employee columns atomically. Running the table creation here
            // would conflict with that migration.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: paired with the empty Up().
        }
    }
}
