namespace Bud.Shared.Contracts.Responses;

public sealed class ObjectiveResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Dimension { get; set; }
    public List<MetricResponse> Metrics { get; set; } = [];
}

public sealed class ObjectiveProgressResponse
{
    public Guid ObjectiveId { get; set; }
    public decimal OverallProgress { get; set; }
    public decimal AverageConfidence { get; set; }
    public int TotalMetrics { get; set; }
    public int MetricsWithCheckins { get; set; }
    public int OutdatedMetrics { get; set; }
}
