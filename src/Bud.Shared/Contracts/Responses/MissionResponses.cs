using Bud.Shared.Contracts;

namespace Bud.Shared.Contracts.Responses;

public sealed class MissionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MissionStatus Status { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? CollaboratorId { get; set; }
    public WorkspaceResponse? Workspace { get; set; }
    public TeamResponse? Team { get; set; }
    public CollaboratorResponse? Collaborator { get; set; }
    public List<MetricResponse> Metrics { get; set; } = [];
    public List<ObjectiveResponse> Objectives { get; set; } = [];
}

public sealed class MissionProgressResponse
{
    public Guid MissionId { get; set; }
    public decimal OverallProgress { get; set; }
    public decimal ExpectedProgress { get; set; }
    public decimal AverageConfidence { get; set; }
    public int TotalMetrics { get; set; }
    public int MetricsWithCheckins { get; set; }
    public int OutdatedMetrics { get; set; }
    public List<ObjectiveProgressResponse> ObjectiveProgress { get; set; } = [];
}
