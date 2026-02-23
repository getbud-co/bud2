namespace Bud.Server.Application.ReadModels;

public sealed class ObjectiveProgressSnapshot
{
    public Guid ObjectiveId { get; set; }
    public decimal OverallProgress { get; set; }
    public decimal AverageConfidence { get; set; }
    public int TotalMetrics { get; set; }
    public int MetricsWithCheckins { get; set; }
    public int OutdatedMetrics { get; set; }
}
