namespace Bud.Server.Application.ReadModels;

public sealed class MissionProgressSnapshot
{
    public Guid MissionId { get; set; }
    public decimal OverallProgress { get; set; }
    public decimal ExpectedProgress { get; set; }
    public decimal AverageConfidence { get; set; }
    public int TotalMetrics { get; set; }
    public int MetricsWithCheckins { get; set; }
    public int OutdatedMetrics { get; set; }
    public List<ObjectiveProgressSnapshot> ObjectiveProgress { get; set; } = [];
}
