using System;
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
                name: "FK_Teams_Workspaces_WorkspaceId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateIndicators_TemplateGoals_TemplateGoalId",
                table: "TemplateIndicators");

            migrationBuilder.DropTable(
                name: "CollaboratorAccessLogs");

            migrationBuilder.DropTable(
                name: "CollaboratorTeams");

            migrationBuilder.DropTable(
                name: "GoalTasks");

            migrationBuilder.DropTable(
                name: "TemplateGoals");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Collaborators");

            migrationBuilder.DropIndex(
                name: "IX_Teams_WorkspaceId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "GoalNamePattern",
                table: "Templates",
                newName: "MissionNamePattern");

            migrationBuilder.RenameColumn(
                name: "GoalDescriptionPattern",
                table: "Templates",
                newName: "MissionDescriptionPattern");

            migrationBuilder.RenameColumn(
                name: "TemplateGoalId",
                table: "TemplateIndicators",
                newName: "TemplateMissionId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateIndicators_TemplateGoalId",
                table: "TemplateIndicators",
                newName: "IX_TemplateIndicators_TemplateMissionId");

            migrationBuilder.RenameColumn(
                name: "RecipientCollaboratorId",
                table: "Notifications",
                newName: "RecipientEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RecipientCollaboratorId_IsRead_CreatedAtUtc",
                table: "Notifications",
                newName: "IX_Notifications_RecipientEmployeeId_IsRead_CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "GoalId",
                table: "Indicators",
                newName: "MissionId");

            migrationBuilder.RenameIndex(
                name: "IX_Indicators_GoalId",
                table: "Indicators",
                newName: "IX_Indicators_MissionId");

            migrationBuilder.RenameColumn(
                name: "CollaboratorId",
                table: "Checkins",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Checkins_CollaboratorId",
                table: "Checkins",
                newName: "IX_Checkins_EmployeeId");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsGlobalAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TemplateMissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Dimension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateMissions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateMissions_TemplateMissions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TemplateMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateMissions_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAccessLogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeAccessLogs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTeams",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTeams", x => new { x.EmployeeId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_EmployeeTeams_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Dimension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Missions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Missions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionTasks_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionTasks_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAccessLogs_EmployeeId",
                table: "EmployeeAccessLogs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAccessLogs_OrganizationId_AccessedAt",
                table: "EmployeeAccessLogs",
                columns: new[] { "OrganizationId", "AccessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_LeaderId",
                table: "Employees",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_OrganizationId",
                table: "Employees",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_TeamId",
                table: "Employees",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTeams_EmployeeId",
                table: "EmployeeTeams",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTeams_TeamId",
                table: "EmployeeTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_EmployeeId",
                table: "Missions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_OrganizationId",
                table: "Missions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ParentId",
                table: "Missions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionTasks_MissionId",
                table: "MissionTasks",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionTasks_MissionId_State",
                table: "MissionTasks",
                columns: new[] { "MissionId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_MissionTasks_OrganizationId",
                table: "MissionTasks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateMissions_OrganizationId",
                table: "TemplateMissions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateMissions_ParentId",
                table: "TemplateMissions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateMissions_TemplateId",
                table: "TemplateMissions",
                column: "TemplateId");

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

            migrationBuilder.DropTable(
                name: "EmployeeAccessLogs");

            migrationBuilder.DropTable(
                name: "EmployeeTeams");

            migrationBuilder.DropTable(
                name: "MissionTasks");

            migrationBuilder.DropTable(
                name: "TemplateMissions");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.RenameColumn(
                name: "MissionNamePattern",
                table: "Templates",
                newName: "GoalNamePattern");

            migrationBuilder.RenameColumn(
                name: "MissionDescriptionPattern",
                table: "Templates",
                newName: "GoalDescriptionPattern");

            migrationBuilder.RenameColumn(
                name: "TemplateMissionId",
                table: "TemplateIndicators",
                newName: "TemplateGoalId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateIndicators_TemplateMissionId",
                table: "TemplateIndicators",
                newName: "IX_TemplateIndicators_TemplateGoalId");

            migrationBuilder.RenameColumn(
                name: "RecipientEmployeeId",
                table: "Notifications",
                newName: "RecipientCollaboratorId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RecipientEmployeeId_IsRead_CreatedAtUtc",
                table: "Notifications",
                newName: "IX_Notifications_RecipientCollaboratorId_IsRead_CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "MissionId",
                table: "Indicators",
                newName: "GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_Indicators_MissionId",
                table: "Indicators",
                newName: "IX_Indicators_GoalId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Checkins",
                newName: "CollaboratorId");

            migrationBuilder.RenameIndex(
                name: "IX_Checkins_EmployeeId",
                table: "Checkins",
                newName: "IX_Checkins_CollaboratorId");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Teams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Collaborators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsGlobalAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collaborators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collaborators_Collaborators_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Collaborators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Collaborators_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Collaborators_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TemplateGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Dimension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateGoals_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateGoals_TemplateGoals_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TemplateGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateGoals_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workspaces_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollaboratorAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollaboratorId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaboratorAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollaboratorAccessLogs_Collaborators_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalTable: "Collaborators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollaboratorAccessLogs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollaboratorTeams",
                columns: table => new
                {
                    CollaboratorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaboratorTeams", x => new { x.CollaboratorId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_CollaboratorTeams_Collaborators_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalTable: "Collaborators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollaboratorTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollaboratorId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Dimension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_Collaborators_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalTable: "Collaborators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goals_Goals_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Goals_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    due_date = table.Column<DateTime>(type: "date", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalTasks_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalTasks_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_WorkspaceId",
                table: "Teams",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaboratorAccessLogs_CollaboratorId",
                table: "CollaboratorAccessLogs",
                column: "CollaboratorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaboratorAccessLogs_OrganizationId_AccessedAt",
                table: "CollaboratorAccessLogs",
                columns: new[] { "OrganizationId", "AccessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Collaborators_Email",
                table: "Collaborators",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collaborators_LeaderId",
                table: "Collaborators",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Collaborators_OrganizationId",
                table: "Collaborators",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Collaborators_TeamId",
                table: "Collaborators",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaboratorTeams_CollaboratorId",
                table: "CollaboratorTeams",
                column: "CollaboratorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaboratorTeams_TeamId",
                table: "CollaboratorTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_CollaboratorId",
                table: "Goals",
                column: "CollaboratorId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_OrganizationId",
                table: "Goals",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_ParentId",
                table: "Goals",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalTasks_GoalId",
                table: "GoalTasks",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalTasks_GoalId_State",
                table: "GoalTasks",
                columns: new[] { "GoalId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_GoalTasks_OrganizationId",
                table: "GoalTasks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateGoals_OrganizationId",
                table: "TemplateGoals",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateGoals_ParentId",
                table: "TemplateGoals",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateGoals_TemplateId",
                table: "TemplateGoals",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_OrganizationId",
                table: "Workspaces",
                column: "OrganizationId");

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
                name: "FK_Teams_Workspaces_WorkspaceId",
                table: "Teams",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
