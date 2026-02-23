namespace Bud.Shared.Contracts.Responses;

public sealed class MyOrganizationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class MyDashboardResponse
{
    public DashboardTeamHealthResponse TeamHealth { get; set; } = new();
    public List<DashboardPendingTaskResponse> PendingTasks { get; set; } = [];
}

public sealed class DashboardTeamHealthResponse
{
    public DashboardLeaderResponse? Leader { get; set; }
    public List<DashboardTeamMemberResponse> TeamMembers { get; set; } = [];
    public DashboardEngagementScoreResponse Engagement { get; set; } = new();
    public DashboardTeamIndicatorsResponse Indicators { get; set; } = new();
}

public sealed class DashboardLeaderResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
}

public sealed class DashboardTeamMemberResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
}

public sealed class DashboardEngagementScoreResponse
{
    public int Score { get; set; }
    public string Level { get; set; } = "low";
    public string Tip { get; set; } = string.Empty;
}

public sealed class DashboardTeamIndicatorsResponse
{
    public DashboardIndicatorResponse WeeklyAccess { get; set; } = new();
    public DashboardIndicatorResponse MissionsUpdated { get; set; } = new();
    public DashboardIndicatorResponse FormsResponded { get; set; } = new();
}

public sealed class DashboardIndicatorResponse
{
    public int Percentage { get; set; }
    public int DeltaPercentage { get; set; }
    public bool IsPlaceholder { get; set; }
}

public sealed class DashboardPendingTaskResponse
{
    public Guid ReferenceId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NavigateUrl { get; set; } = string.Empty;
}
