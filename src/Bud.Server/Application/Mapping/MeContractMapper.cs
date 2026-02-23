using Bud.Server.Application.ReadModels;
using Bud.Server.Domain.Model;

namespace Bud.Server.Application.Mapping;

internal static class MeContractMapper
{
    public static MyOrganizationResponse ToResponse(this OrganizationSnapshot source)
    {
        return new MyOrganizationResponse
        {
            Id = source.Id,
            Name = source.Name
        };
    }

    public static MyDashboardResponse ToResponse(this DashboardSnapshot source)
    {
        return new MyDashboardResponse
        {
            TeamHealth = source.TeamHealth.ToResponse(),
            PendingTasks = source.PendingTasks.Select(ToResponse).ToList()
        };
    }

    private static DashboardTeamHealthResponse ToResponse(this TeamHealthSnapshot source)
    {
        return new DashboardTeamHealthResponse
        {
            Leader = source.Leader?.ToResponse(),
            TeamMembers = source.TeamMembers.Select(ToResponse).ToList(),
            Engagement = source.Engagement.ToResponse(),
            Indicators = new DashboardTeamIndicatorsResponse
            {
                WeeklyAccess = source.WeeklyAccess.ToResponse(),
                MissionsUpdated = source.MissionsUpdated.ToResponse(),
                FormsResponded = source.FormsResponded.ToResponse()
            }
        };
    }

    private static DashboardLeaderResponse ToResponse(this TeamLeaderSnapshot source)
    {
        return new DashboardLeaderResponse
        {
            Id = source.Id,
            FullName = source.FullName,
            Initials = source.Initials,
            Role = source.Role,
            TeamName = source.TeamName
        };
    }

    private static DashboardTeamMemberResponse ToResponse(this TeamMemberSnapshot source)
    {
        return new DashboardTeamMemberResponse
        {
            Id = source.Id,
            FullName = source.FullName,
            Initials = source.Initials
        };
    }

    private static DashboardEngagementScoreResponse ToResponse(this EngagementScore source)
    {
        return new DashboardEngagementScoreResponse
        {
            Score = source.Score,
            Level = source.Level,
            Tip = source.Tip
        };
    }

    private static DashboardIndicatorResponse ToResponse(this PerformanceIndicator source)
    {
        return new DashboardIndicatorResponse
        {
            Percentage = source.Percentage,
            DeltaPercentage = source.DeltaPercentage,
            IsPlaceholder = source.IsPlaceholder
        };
    }

    private static DashboardPendingTaskResponse ToResponse(this PendingTaskSnapshot source)
    {
        return new DashboardPendingTaskResponse
        {
            ReferenceId = source.ReferenceId,
            TaskType = source.TaskType,
            Title = source.Title,
            Description = source.Description,
            NavigateUrl = source.NavigateUrl
        };
    }
}
