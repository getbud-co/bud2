using Bud.Shared.Contracts.Features.Tags;
using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Missions;

public sealed class MissionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Dimension { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MissionStatus Status { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public EmployeeResponse? Employee { get; set; }
    public List<MissionResponse> Children { get; set; } = [];
    public List<IndicatorResponse> Indicators { get; set; } = [];
    public List<TaskResponse> Tasks { get; set; } = [];
    public List<TagResponse> Tags { get; set; } = [];
}

public sealed class MissionProgressResponse
{
    public Guid MissionId { get; set; }
    public decimal OverallProgress { get; set; }
    public decimal ExpectedProgress { get; set; }
    public decimal AverageConfidence { get; set; }
    public int TotalIndicators { get; set; }
    public int IndicatorsWithCheckins { get; set; }
    public int OutdatedIndicators { get; set; }
    public int DirectChildren { get; set; }
    public int DirectIndicators { get; set; }
    public int TodoTasks { get; set; }
    public int DoingTasks { get; set; }
    public DateTime? LastCheckinDate { get; set; }
    public List<Guid> DistinctEmployeeIds { get; set; } = [];
}
