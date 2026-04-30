using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncSnapshot2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop external FKs that point to tables being renamed
            migrationBuilder.DropForeignKey(
                name: "FK_Checkins_Collaborators_CollaboratorId",
                table: "Checkins");

            migrationBuilder.DropForeignKey(
                name: "FK_Indicators_Goals_GoalId",
                table: "Indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Collaborators_RecipientCollaboratorId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Collaborators_LeaderId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateIndicators_TemplateGoals_TemplateGoalId",
                table: "TemplateIndicators");

            // 2. Rename tables
            migrationBuilder.RenameTable(
                name: "Collaborators",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "CollaboratorTeams",
                newName: "EmployeeTeams");

            migrationBuilder.RenameTable(
                name: "CollaboratorAccessLogs",
                newName: "EmployeeAccessLogs");

            migrationBuilder.RenameTable(
                name: "Goals",
                newName: "Missions");

            migrationBuilder.RenameTable(
                name: "GoalTasks",
                newName: "MissionTasks");

            migrationBuilder.RenameTable(
                name: "TemplateGoals",
                newName: "TemplateMissions");

            // 3. Rename PK and internal FK constraints via raw SQL (PostgreSQL doesn't auto-rename them)
            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""PK_Collaborators"" TO ""PK_Employees"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""FK_Collaborators_Collaborators_LeaderId"" TO ""FK_Employees_Employees_LeaderId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""FK_Collaborators_Organizations_OrganizationId"" TO ""FK_Employees_Organizations_OrganizationId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""FK_Collaborators_Teams_TeamId"" TO ""FK_Employees_Teams_TeamId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeTeams"" RENAME CONSTRAINT ""PK_CollaboratorTeams"" TO ""PK_EmployeeTeams"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeTeams"" RENAME CONSTRAINT ""FK_CollaboratorTeams_Collaborators_CollaboratorId"" TO ""FK_EmployeeTeams_Employees_EmployeeId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeTeams"" RENAME CONSTRAINT ""FK_CollaboratorTeams_Teams_TeamId"" TO ""FK_EmployeeTeams_Teams_TeamId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeAccessLogs"" RENAME CONSTRAINT ""PK_CollaboratorAccessLogs"" TO ""PK_EmployeeAccessLogs"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeAccessLogs"" RENAME CONSTRAINT ""FK_CollaboratorAccessLogs_Collaborators_CollaboratorId"" TO ""FK_EmployeeAccessLogs_Employees_EmployeeId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeAccessLogs"" RENAME CONSTRAINT ""FK_CollaboratorAccessLogs_Organizations_OrganizationId"" TO ""FK_EmployeeAccessLogs_Organizations_OrganizationId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""PK_Goals"" TO ""PK_Missions"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""FK_Goals_Collaborators_CollaboratorId"" TO ""FK_Missions_Employees_EmployeeId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""FK_Goals_Goals_ParentId"" TO ""FK_Missions_Missions_ParentId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""FK_Goals_Organizations_OrganizationId"" TO ""FK_Missions_Organizations_OrganizationId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""MissionTasks"" RENAME CONSTRAINT ""PK_GoalTasks"" TO ""PK_MissionTasks"";");
            migrationBuilder.Sql(@"ALTER TABLE ""MissionTasks"" RENAME CONSTRAINT ""FK_GoalTasks_Goals_GoalId"" TO ""FK_MissionTasks_Missions_MissionId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""MissionTasks"" RENAME CONSTRAINT ""FK_GoalTasks_Organizations_OrganizationId"" TO ""FK_MissionTasks_Organizations_OrganizationId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""PK_TemplateGoals"" TO ""PK_TemplateMissions"";");
            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""FK_TemplateGoals_Organizations_OrganizationId"" TO ""FK_TemplateMissions_Organizations_OrganizationId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""FK_TemplateGoals_TemplateGoals_ParentId"" TO ""FK_TemplateMissions_TemplateMissions_ParentId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""FK_TemplateGoals_Templates_TemplateId"" TO ""FK_TemplateMissions_Templates_TemplateId"";");

            // 4. Rename columns in renamed tables
            migrationBuilder.RenameColumn(
                name: "CollaboratorId",
                table: "EmployeeTeams",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "CollaboratorId",
                table: "EmployeeAccessLogs",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "CollaboratorId",
                table: "Missions",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "GoalId",
                table: "MissionTasks",
                newName: "MissionId");

            // 5. Rename columns in other (non-renamed) tables
            migrationBuilder.RenameColumn(
                name: "CollaboratorId",
                table: "Checkins",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "GoalId",
                table: "Indicators",
                newName: "MissionId");

            migrationBuilder.RenameColumn(
                name: "RecipientCollaboratorId",
                table: "Notifications",
                newName: "RecipientEmployeeId");

            migrationBuilder.RenameColumn(
                name: "TemplateGoalId",
                table: "TemplateIndicators",
                newName: "TemplateMissionId");

            migrationBuilder.RenameColumn(
                name: "GoalNamePattern",
                table: "Templates",
                newName: "MissionNamePattern");

            migrationBuilder.RenameColumn(
                name: "GoalDescriptionPattern",
                table: "Templates",
                newName: "MissionDescriptionPattern");

            // 6. Rename indexes (Employees)
            migrationBuilder.RenameIndex(
                name: "IX_Collaborators_Email",
                table: "Employees",
                newName: "IX_Employees_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Collaborators_LeaderId",
                table: "Employees",
                newName: "IX_Employees_LeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_Collaborators_OrganizationId",
                table: "Employees",
                newName: "IX_Employees_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Collaborators_TeamId",
                table: "Employees",
                newName: "IX_Employees_TeamId");

            // Rename indexes (EmployeeTeams)
            migrationBuilder.RenameIndex(
                name: "IX_CollaboratorTeams_CollaboratorId",
                table: "EmployeeTeams",
                newName: "IX_EmployeeTeams_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CollaboratorTeams_TeamId",
                table: "EmployeeTeams",
                newName: "IX_EmployeeTeams_TeamId");

            // Rename indexes (EmployeeAccessLogs)
            migrationBuilder.RenameIndex(
                name: "IX_CollaboratorAccessLogs_CollaboratorId",
                table: "EmployeeAccessLogs",
                newName: "IX_EmployeeAccessLogs_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CollaboratorAccessLogs_OrganizationId_AccessedAt",
                table: "EmployeeAccessLogs",
                newName: "IX_EmployeeAccessLogs_OrganizationId_AccessedAt");

            // Rename indexes (Missions)
            migrationBuilder.RenameIndex(
                name: "IX_Goals_CollaboratorId",
                table: "Missions",
                newName: "IX_Missions_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Goals_OrganizationId",
                table: "Missions",
                newName: "IX_Missions_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Goals_ParentId",
                table: "Missions",
                newName: "IX_Missions_ParentId");

            // Rename indexes (MissionTasks)
            migrationBuilder.RenameIndex(
                name: "IX_GoalTasks_GoalId",
                table: "MissionTasks",
                newName: "IX_MissionTasks_MissionId");

            migrationBuilder.RenameIndex(
                name: "IX_GoalTasks_GoalId_State",
                table: "MissionTasks",
                newName: "IX_MissionTasks_MissionId_State");

            migrationBuilder.RenameIndex(
                name: "IX_GoalTasks_OrganizationId",
                table: "MissionTasks",
                newName: "IX_MissionTasks_OrganizationId");

            // Rename indexes (TemplateMissions)
            migrationBuilder.RenameIndex(
                name: "IX_TemplateGoals_OrganizationId",
                table: "TemplateMissions",
                newName: "IX_TemplateMissions_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateGoals_ParentId",
                table: "TemplateMissions",
                newName: "IX_TemplateMissions_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateGoals_TemplateId",
                table: "TemplateMissions",
                newName: "IX_TemplateMissions_TemplateId");

            // Rename indexes (other tables)
            migrationBuilder.RenameIndex(
                name: "IX_Checkins_CollaboratorId",
                table: "Checkins",
                newName: "IX_Checkins_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Indicators_GoalId",
                table: "Indicators",
                newName: "IX_Indicators_MissionId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RecipientCollaboratorId_IsRead_CreatedAtUtc",
                table: "Notifications",
                newName: "IX_Notifications_RecipientEmployeeId_IsRead_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateIndicators_TemplateGoalId",
                table: "TemplateIndicators",
                newName: "IX_TemplateIndicators_TemplateMissionId");

            // 7. Re-add external FKs with new names pointing to renamed tables/columns
            migrationBuilder.AddForeignKey(
                name: "FK_Checkins_Employees_EmployeeId",
                table: "Checkins",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Indicators_Missions_MissionId",
                table: "Indicators",
                column: "MissionId",
                principalTable: "Missions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Employees_RecipientEmployeeId",
                table: "Notifications",
                column: "RecipientEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Employees_LeaderId",
                table: "Teams",
                column: "LeaderId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateIndicators_TemplateMissions_TemplateMissionId",
                table: "TemplateIndicators",
                column: "TemplateMissionId",
                principalTable: "TemplateMissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop external FKs (new names)
            migrationBuilder.DropForeignKey(
                name: "FK_Checkins_Employees_EmployeeId",
                table: "Checkins");

            migrationBuilder.DropForeignKey(
                name: "FK_Indicators_Missions_MissionId",
                table: "Indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Employees_RecipientEmployeeId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Employees_LeaderId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateIndicators_TemplateMissions_TemplateMissionId",
                table: "TemplateIndicators");

            // 2. Rename columns back in other tables
            migrationBuilder.RenameColumn(
                name: "MissionDescriptionPattern",
                table: "Templates",
                newName: "GoalDescriptionPattern");

            migrationBuilder.RenameColumn(
                name: "MissionNamePattern",
                table: "Templates",
                newName: "GoalNamePattern");

            migrationBuilder.RenameColumn(
                name: "TemplateMissionId",
                table: "TemplateIndicators",
                newName: "TemplateGoalId");

            migrationBuilder.RenameColumn(
                name: "RecipientEmployeeId",
                table: "Notifications",
                newName: "RecipientCollaboratorId");

            migrationBuilder.RenameColumn(
                name: "MissionId",
                table: "Indicators",
                newName: "GoalId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Checkins",
                newName: "CollaboratorId");

            // 3. Rename indexes back (other tables)
            migrationBuilder.RenameIndex(
                name: "IX_TemplateIndicators_TemplateMissionId",
                table: "TemplateIndicators",
                newName: "IX_TemplateIndicators_TemplateGoalId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RecipientEmployeeId_IsRead_CreatedAtUtc",
                table: "Notifications",
                newName: "IX_Notifications_RecipientCollaboratorId_IsRead_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Indicators_MissionId",
                table: "Indicators",
                newName: "IX_Indicators_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_Checkins_EmployeeId",
                table: "Checkins",
                newName: "IX_Checkins_CollaboratorId");

            // 4. Rename columns back in renamed tables (before table rename)
            migrationBuilder.RenameColumn(
                name: "MissionId",
                table: "MissionTasks",
                newName: "GoalId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Missions",
                newName: "CollaboratorId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeAccessLogs",
                newName: "CollaboratorId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeTeams",
                newName: "CollaboratorId");

            // 5. Rename indexes back in renamed tables
            migrationBuilder.RenameIndex(
                name: "IX_TemplateMissions_TemplateId",
                table: "TemplateMissions",
                newName: "IX_TemplateGoals_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateMissions_ParentId",
                table: "TemplateMissions",
                newName: "IX_TemplateGoals_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateMissions_OrganizationId",
                table: "TemplateMissions",
                newName: "IX_TemplateGoals_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_MissionTasks_OrganizationId",
                table: "MissionTasks",
                newName: "IX_GoalTasks_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_MissionTasks_MissionId_State",
                table: "MissionTasks",
                newName: "IX_GoalTasks_GoalId_State");

            migrationBuilder.RenameIndex(
                name: "IX_MissionTasks_MissionId",
                table: "MissionTasks",
                newName: "IX_GoalTasks_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_ParentId",
                table: "Missions",
                newName: "IX_Goals_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_OrganizationId",
                table: "Missions",
                newName: "IX_Goals_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_EmployeeId",
                table: "Missions",
                newName: "IX_Goals_CollaboratorId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAccessLogs_OrganizationId_AccessedAt",
                table: "EmployeeAccessLogs",
                newName: "IX_CollaboratorAccessLogs_OrganizationId_AccessedAt");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAccessLogs_EmployeeId",
                table: "EmployeeAccessLogs",
                newName: "IX_CollaboratorAccessLogs_CollaboratorId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTeams_TeamId",
                table: "EmployeeTeams",
                newName: "IX_CollaboratorTeams_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTeams_EmployeeId",
                table: "EmployeeTeams",
                newName: "IX_CollaboratorTeams_CollaboratorId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_TeamId",
                table: "Employees",
                newName: "IX_Collaborators_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_OrganizationId",
                table: "Employees",
                newName: "IX_Collaborators_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_LeaderId",
                table: "Employees",
                newName: "IX_Collaborators_LeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                newName: "IX_Collaborators_Email");

            // 6. Rename constraints back via raw SQL
            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""PK_TemplateMissions"" TO ""PK_TemplateGoals"";");
            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""FK_TemplateMissions_Templates_TemplateId"" TO ""FK_TemplateGoals_Templates_TemplateId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""FK_TemplateMissions_TemplateMissions_ParentId"" TO ""FK_TemplateGoals_TemplateGoals_ParentId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""TemplateMissions"" RENAME CONSTRAINT ""FK_TemplateMissions_Organizations_OrganizationId"" TO ""FK_TemplateGoals_Organizations_OrganizationId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""MissionTasks"" RENAME CONSTRAINT ""PK_MissionTasks"" TO ""PK_GoalTasks"";");
            migrationBuilder.Sql(@"ALTER TABLE ""MissionTasks"" RENAME CONSTRAINT ""FK_MissionTasks_Organizations_OrganizationId"" TO ""FK_GoalTasks_Organizations_OrganizationId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""MissionTasks"" RENAME CONSTRAINT ""FK_MissionTasks_Missions_MissionId"" TO ""FK_GoalTasks_Goals_GoalId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""PK_Missions"" TO ""PK_Goals"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""FK_Missions_Organizations_OrganizationId"" TO ""FK_Goals_Organizations_OrganizationId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""FK_Missions_Missions_ParentId"" TO ""FK_Goals_Goals_ParentId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Missions"" RENAME CONSTRAINT ""FK_Missions_Employees_EmployeeId"" TO ""FK_Goals_Collaborators_CollaboratorId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeAccessLogs"" RENAME CONSTRAINT ""PK_EmployeeAccessLogs"" TO ""PK_CollaboratorAccessLogs"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeAccessLogs"" RENAME CONSTRAINT ""FK_EmployeeAccessLogs_Organizations_OrganizationId"" TO ""FK_CollaboratorAccessLogs_Organizations_OrganizationId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeAccessLogs"" RENAME CONSTRAINT ""FK_EmployeeAccessLogs_Employees_EmployeeId"" TO ""FK_CollaboratorAccessLogs_Collaborators_CollaboratorId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeTeams"" RENAME CONSTRAINT ""PK_EmployeeTeams"" TO ""PK_CollaboratorTeams"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeTeams"" RENAME CONSTRAINT ""FK_EmployeeTeams_Teams_TeamId"" TO ""FK_CollaboratorTeams_Teams_TeamId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""EmployeeTeams"" RENAME CONSTRAINT ""FK_EmployeeTeams_Employees_EmployeeId"" TO ""FK_CollaboratorTeams_Collaborators_CollaboratorId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""PK_Employees"" TO ""PK_Collaborators"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""FK_Employees_Teams_TeamId"" TO ""FK_Collaborators_Teams_TeamId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""FK_Employees_Organizations_OrganizationId"" TO ""FK_Collaborators_Organizations_OrganizationId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Employees"" RENAME CONSTRAINT ""FK_Employees_Employees_LeaderId"" TO ""FK_Collaborators_Collaborators_LeaderId"";");

            // 7. Rename tables back
            migrationBuilder.RenameTable(
                name: "TemplateMissions",
                newName: "TemplateGoals");

            migrationBuilder.RenameTable(
                name: "MissionTasks",
                newName: "GoalTasks");

            migrationBuilder.RenameTable(
                name: "Missions",
                newName: "Goals");

            migrationBuilder.RenameTable(
                name: "EmployeeAccessLogs",
                newName: "CollaboratorAccessLogs");

            migrationBuilder.RenameTable(
                name: "EmployeeTeams",
                newName: "CollaboratorTeams");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "Collaborators");

            // 8. Re-add external FKs with old names
            migrationBuilder.AddForeignKey(
                name: "FK_Checkins_Collaborators_CollaboratorId",
                table: "Checkins",
                column: "CollaboratorId",
                principalTable: "Collaborators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Indicators_Goals_GoalId",
                table: "Indicators",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Collaborators_RecipientCollaboratorId",
                table: "Notifications",
                column: "RecipientCollaboratorId",
                principalTable: "Collaborators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Collaborators_LeaderId",
                table: "Teams",
                column: "LeaderId",
                principalTable: "Collaborators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateIndicators_TemplateGoals_TemplateGoalId",
                table: "TemplateIndicators",
                column: "TemplateGoalId",
                principalTable: "TemplateGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
