using Bud.Infrastructure.Persistence;
using Bud.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Me;

public sealed class DashboardReadStore(ApplicationDbContext dbContext) : IMyDashboardReadStore
{
    public async Task<DashboardSnapshot?> GetMyDashboardAsync(
        Guid employeeId,
        Guid? teamId,
        CancellationToken ct = default)
    {
        var employee = await dbContext.Employees
            .AsNoTracking()
            .Include(c => c.Leader)
            .FirstOrDefaultAsync(c => c.Id == employeeId, ct);

        if (employee is null)
        {
            return null;
        }

        Employee? leaderSource;
        List<Employee> teamMembers;
        string? teamNameOverride = null;

        if (teamId.HasValue)
        {
            var team = await dbContext.Teams
                .AsNoTracking()
                .Include(t => t.Leader)
                .FirstOrDefaultAsync(t => t.Id == teamId.Value, ct);

            leaderSource = team?.Leader;
            teamNameOverride = team?.Name;

            var memberIds = await dbContext.EmployeeTeams
                .AsNoTracking()
                .Where(cteam => cteam.TeamId == teamId.Value)
                .Select(cteam => cteam.EmployeeId)
                .ToListAsync(ct);

            teamMembers = memberIds.Count > 0
                ? await dbContext.Employees
                    .AsNoTracking()
                    .Where(c => memberIds.Contains(c.Id))
                    .ToListAsync(ct)
                : [];
        }
        else
        {
            var teamIds = await dbContext.EmployeeTeams
                .AsNoTracking()
                .Where(cteam => cteam.EmployeeId == employee.Id)
                .Select(cteam => cteam.TeamId)
                .Distinct()
                .ToListAsync(ct);

            if (teamIds.Count > 0)
            {
                var teams = await dbContext.Teams
                    .AsNoTracking()
                    .Include(t => t.Leader)
                    .Where(t => teamIds.Contains(t.Id))
                    .ToListAsync(ct);

                var memberIds = await dbContext.EmployeeTeams
                    .AsNoTracking()
                    .Where(cteam => teamIds.Contains(cteam.TeamId))
                    .Select(cteam => cteam.EmployeeId)
                    .Distinct()
                    .ToListAsync(ct);

                teamMembers = memberIds.Count > 0
                    ? await dbContext.Employees
                        .AsNoTracking()
                        .Where(c => memberIds.Contains(c.Id))
                        .ToListAsync(ct)
                    : [];

                if (teams.Count == 1)
                {
                    leaderSource = teams[0].Leader;
                    teamNameOverride = teams[0].Name;
                }
                else
                {
                    leaderSource = employee.Leader
                        ?? (employee.Role == EmployeeRole.Leader ? employee : null);
                }
            }
            else
            {
                leaderSource = employee.Leader
                    ?? (employee.Role == EmployeeRole.Leader ? employee : null);

                if (leaderSource is not null)
                {
                    var leaderTeamNames = await dbContext.EmployeeTeams
                        .AsNoTracking()
                        .Where(cteam => cteam.EmployeeId == leaderSource.Id)
                        .Include(cteam => cteam.Team)
                        .Select(cteam => cteam.Team.Name)
                        .Distinct()
                        .ToListAsync(ct);

                    if (leaderTeamNames.Count == 1)
                    {
                        teamNameOverride = leaderTeamNames[0];
                    }
                }

                teamMembers = [];
            }
        }

        if (leaderSource is not null)
        {
            teamMembers = teamMembers
                .Where(member => member.Id != leaderSource.Id)
                .ToList();
        }

        var teamHealth = await BuildTeamHealthAsync(leaderSource, teamMembers, employee.Id, employee.OrganizationId, teamNameOverride, ct);
        var pendingTasks = await BuildPendingTasksAsync(employee, ct);

        return new DashboardSnapshot
        {
            TeamHealth = teamHealth,
            PendingTasks = pendingTasks
        };
    }

    private async Task<TeamHealthSnapshot> BuildTeamHealthAsync(
        Employee? leaderSource,
        List<Employee> directReports,
        Guid currentEmployeeId,
        Guid organizationId,
        string? teamNameOverride,
        CancellationToken ct)
    {
        var leader = BuildLeaderDto(leaderSource, teamNameOverride);
        var teamMemberDtos = BuildTeamMembers(directReports);
        var teamMemberIds = directReports.Select(m => m.Id).ToList();

        // Indicadores sempre incluem o próprio usuário
        var indicatorMemberIds = new HashSet<Guid>(teamMemberIds) { currentEmployeeId };
        var indicatorMemberIdList = indicatorMemberIds.ToList();

        var weeklyAccess = await CalculateWeeklyAccessAsync(indicatorMemberIdList, organizationId, ct);
        var missionsUpdated = await CalculateMissionsUpdatedAsync(indicatorMemberIdList, organizationId, ct);
        var formsResponded = PerformanceIndicator.Placeholder();

        var avgConfidence = await CalculateAverageConfidenceAsync(indicatorMemberIdList, ct);
        var engagement = CalculateEngagement(weeklyAccess.Percentage, missionsUpdated.Percentage, avgConfidence);

        return new TeamHealthSnapshot
        {
            Leader = leader,
            TeamMembers = teamMemberDtos,
            Engagement = engagement,
            WeeklyAccess = weeklyAccess,
            MissionsUpdated = missionsUpdated,
            FormsResponded = formsResponded
        };
    }

    private static TeamLeaderSnapshot? BuildLeaderDto(Employee? leaderSource, string? teamNameOverride = null)
    {
        if (leaderSource is null)
        {
            return null;
        }

        return new TeamLeaderSnapshot
        {
            Id = leaderSource.Id,
            FullName = leaderSource.FullName,
            Initials = GetInitials(leaderSource.FullName),
            Role = leaderSource.Role == EmployeeRole.Leader ? "Líder" : "Colaborador",
            TeamName = teamNameOverride ?? leaderSource.Team?.Name ?? string.Empty
        };
    }

    private static List<TeamMemberSnapshot> BuildTeamMembers(List<Employee> members)
    {
        return members.Select(m => new TeamMemberSnapshot
        {
            Id = m.Id,
            FullName = m.FullName,
            Initials = GetInitials(m.FullName)
        }).ToList();
    }

    private async Task<PerformanceIndicator> CalculateWeeklyAccessAsync(
        List<Guid> teamMemberIds,
        Guid organizationId,
        CancellationToken ct)
    {
        if (teamMemberIds.Count == 0)
        {
            return PerformanceIndicator.Zero();
        }

        var now = DateTime.UtcNow;
        var thisWeekStart = now.AddDays(-7);
        var lastWeekStart = now.AddDays(-14);

        var thisWeekCount = await dbContext.EmployeeAccessLogs
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId
                && teamMemberIds.Contains(l.EmployeeId)
                && l.AccessedAt >= thisWeekStart)
            .Select(l => l.EmployeeId)
            .Distinct()
            .CountAsync(ct);

        var lastWeekCount = await dbContext.EmployeeAccessLogs
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId
                && teamMemberIds.Contains(l.EmployeeId)
                && l.AccessedAt >= lastWeekStart
                && l.AccessedAt < thisWeekStart)
            .Select(l => l.EmployeeId)
            .Distinct()
            .CountAsync(ct);

        var total = teamMemberIds.Count;
        var currentPct = (int)Math.Round(thisWeekCount * 100.0 / total);
        var previousPct = (int)Math.Round(lastWeekCount * 100.0 / total);

        return PerformanceIndicator.Create(currentPct, currentPct - previousPct);
    }

    private async Task<PerformanceIndicator> CalculateMissionsUpdatedAsync(
        List<Guid> teamMemberIds,
        Guid organizationId,
        CancellationToken ct)
    {
        if (teamMemberIds.Count == 0)
        {
            return PerformanceIndicator.Zero();
        }

        var now = DateTime.UtcNow;
        var thisWeekStart = now.AddDays(-7);
        var lastWeekStart = now.AddDays(-14);

        var activeMissionIds = await BuildMyActiveMissionsQuery(teamMemberIds, organizationId)
            .Select(g => g.Id)
            .ToListAsync(ct);

        if (activeMissionIds.Count == 0)
        {
            return PerformanceIndicator.Zero();
        }

        var indicatorIdsForActiveMissions = await dbContext.Indicators
            .AsNoTracking()
            .Where(i => activeMissionIds.Contains(i.MissionId))
            .Select(i => i.Id)
            .ToListAsync(ct);

        // Missions updated via checkins this week
        var thisWeekUpdatedViaCheckins = indicatorIdsForActiveMissions.Count > 0
            ? await dbContext.Checkins
                .AsNoTracking()
                .Where(c => indicatorIdsForActiveMissions.Contains(c.IndicatorId)
                    && c.CheckinDate >= thisWeekStart)
                .Select(c => c.Indicator.MissionId)
                .Distinct()
                .ToListAsync(ct)
            : new List<Guid>();

        // Missions updated via Doing tasks (considered "atualizada")
        var missionsWithDoingTasks = await dbContext.MissionTasks
            .AsNoTracking()
            .Where(t => activeMissionIds.Contains(t.MissionId) && t.State == TaskState.Doing)
            .Select(t => t.MissionId)
            .Distinct()
            .ToListAsync(ct);

        var thisWeekUpdatedMissionIds = thisWeekUpdatedViaCheckins
            .Union(missionsWithDoingTasks)
            .Intersect(activeMissionIds)
            .ToHashSet();

        // Last week: only checkin-based (tasks are a current snapshot, not historical)
        var lastWeekUpdated = indicatorIdsForActiveMissions.Count > 0
            ? await dbContext.Checkins
                .AsNoTracking()
                .Where(c => indicatorIdsForActiveMissions.Contains(c.IndicatorId)
                    && c.CheckinDate >= lastWeekStart
                    && c.CheckinDate < thisWeekStart)
                .Select(c => c.Indicator.MissionId)
                .Distinct()
                .CountAsync(ct)
            : 0;

        var totalActive = activeMissionIds.Count;
        if (totalActive == 0)
        {
            return PerformanceIndicator.Zero();
        }

        var currentPct = (int)Math.Round(thisWeekUpdatedMissionIds.Count * 100.0 / totalActive);
        var previousPct = (int)Math.Round(lastWeekUpdated * 100.0 / totalActive);

        return PerformanceIndicator.Create(currentPct, currentPct - previousPct);
    }

    private async Task<int> CalculateAverageConfidenceAsync(
        List<Guid> teamMemberIds,
        CancellationToken ct)
    {
        if (teamMemberIds.Count == 0)
        {
            return 0;
        }

        var recentCheckins = await dbContext.Checkins
            .AsNoTracking()
            .Where(c => teamMemberIds.Contains(c.EmployeeId)
                && c.CheckinDate >= DateTime.UtcNow.AddDays(-30)
                && c.ConfidenceLevel > 0)
            .Select(c => c.ConfidenceLevel)
            .ToListAsync(ct);

        if (recentCheckins.Count == 0)
        {
            return 0;
        }

        var avg = recentCheckins.Average();
        return (int)Math.Round((avg / 5.0) * 100);
    }

    private static EngagementScore CalculateEngagement(int weeklyAccessPct, int missionsUpdatedPct, int confidencePct)
    {
        var score = (int)Math.Round(weeklyAccessPct * 0.30 + missionsUpdatedPct * 0.40 + confidencePct * 0.30);
        score = Math.Clamp(score, 0, 100);

        return EngagementScore.Create(score);
    }

    private IQueryable<Mission> BuildMyActiveMissionsQuery(
        List<Guid> memberIds,
        Guid organizationId)
    {
        return dbContext.Missions
            .AsNoTracking()
            .Where(g => g.Status == MissionStatus.Active
                && (memberIds.Contains(g.EmployeeId ?? Guid.Empty)
                    || (g.OrganizationId == organizationId && g.EmployeeId == null)));
    }

    private async Task<List<PendingTaskSnapshot>> BuildPendingTasksAsync(
        Employee employee,
        CancellationToken ct)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var memberIds = new List<Guid> { employee.Id };
        var myMissions = await BuildMyActiveMissionsQuery(memberIds, employee.OrganizationId)
            .Include(g => g.Indicators)
                .ThenInclude(i => i.Checkins)
            .ToListAsync(ct);

        var tasks = new List<PendingTaskSnapshot>();

        foreach (var mission in myMissions)
        {
            var needsCheckin = mission.Indicators.Any(i =>
                !i.Checkins.Any(c => c.CheckinDate >= sevenDaysAgo));

            if (needsCheckin)
            {
                tasks.Add(new PendingTaskSnapshot
                {
                    ReferenceId = mission.Id,
                    TaskType = "mission_checkin",
                    Title = mission.Name,
                    Description = "Check-in pendente há mais de 7 dias",
                    NavigateUrl = $"/missions/{mission.Id}"
                });
            }
        }

        // Add active MissionTasks (ToDo and Doing) from accessible missions
        var myMissionIds = myMissions.Select(g => g.Id).ToList();
        if (myMissionIds.Count > 0)
        {
            var activeTasks = await dbContext.MissionTasks
                .AsNoTracking()
                .Where(t => myMissionIds.Contains(t.MissionId)
                    && (t.State == TaskState.ToDo || t.State == TaskState.Doing))
                .ToListAsync(ct);

            var missionNameById = myMissions.ToDictionary(g => g.Id, g => g.Name);
            foreach (var missionTask in activeTasks)
            {
                var missionName = missionNameById.GetValueOrDefault(missionTask.MissionId, string.Empty);
                tasks.Add(new PendingTaskSnapshot
                {
                    ReferenceId = missionTask.Id,
                    TaskType = "mission_task",
                    Title = missionTask.Name,
                    Description = $"Meta: {missionName} — {GetTaskStateLabel(missionTask.State)}",
                    NavigateUrl = $"/missions/{missionTask.MissionId}"
                });
            }
        }

        return tasks;
    }

    private static string GetTaskStateLabel(TaskState state) => state switch
    {
        TaskState.ToDo => "A fazer",
        TaskState.Doing => "Em andamento",
        _ => string.Empty
    };

    private static string GetInitials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        if (parts.Length == 1)
        {
            return parts[0][..1].ToUpperInvariant();
        }

        return $"{parts[0][..1]}{parts[^1][..1]}".ToUpperInvariant();
    }
}
