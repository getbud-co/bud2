using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseEmployeeAsAggregateRoot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Memberships table may not exist if prior migrations were recorded in
            // __EFMigrationsHistory without being physically applied (e.g. after an
            // EnsureCreated() cycle in development). Create it in its final shape if absent.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Memberships"" (
                    ""EmployeeId""     uuid    NOT NULL,
                    ""OrganizationId"" uuid    NOT NULL,
                    ""Role""           integer NOT NULL DEFAULT 0,
                    ""LeaderId""       uuid,
                    ""IsGlobalAdmin""  boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_Memberships"" PRIMARY KEY (""EmployeeId"", ""OrganizationId""),
                    CONSTRAINT ""FK_Memberships_Employees_EmployeeId""
                        FOREIGN KEY (""EmployeeId"") REFERENCES ""Employees""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_Memberships_Organizations_OrganizationId""
                        FOREIGN KEY (""OrganizationId"") REFERENCES ""Organizations""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_Memberships_Employees_LeaderId""
                        FOREIGN KEY (""LeaderId"") REFERENCES ""Employees""(""Id"") ON DELETE RESTRICT
                );
                CREATE INDEX IF NOT EXISTS ""IX_Memberships_OrganizationId""
                    ON ""Memberships"" (""OrganizationId"");
                CREATE INDEX IF NOT EXISTS ""IX_Memberships_LeaderId""
                    ON ""Memberships"" (""LeaderId"");
            ");

            // If TeamId still exists (from an older version of UpdateEmployeeSchema), drop it.
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Memberships' AND column_name = 'TeamId'
                    ) THEN
                        ALTER TABLE ""Memberships"" DROP CONSTRAINT IF EXISTS ""FK_Memberships_Teams_TeamId"";
                        DROP INDEX IF EXISTS ""IX_Memberships_TeamId"";
                        ALTER TABLE ""Memberships"" DROP COLUMN ""TeamId"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "Memberships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_TeamId",
                table: "Memberships",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Teams_TeamId",
                table: "Memberships",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
