using Bud.Shared.Contracts;

namespace Bud.Shared.Contracts.Responses;

public sealed class MetricResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MissionId { get; set; }
    public Guid? ObjectiveId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MetricType Type { get; set; }
    public QuantitativeMetricType? QuantitativeType { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public MetricUnit? Unit { get; set; }
    public string? TargetText { get; set; }
    public ObjectiveResponse? Objective { get; set; }
    public List<MetricCheckinResponse> Checkins { get; set; } = [];
}

public sealed class MetricCheckinResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MetricId { get; set; }
    public Guid CollaboratorId { get; set; }
    public decimal? Value { get; set; }
    public string? Text { get; set; }
    public DateTime CheckinDate { get; set; }
    public string? Note { get; set; }
    public int ConfidenceLevel { get; set; }
    public CollaboratorResponse? Collaborator { get; set; }
}

public sealed class MetricProgressResponse
{
    public Guid MetricId { get; set; }
    public decimal Progress { get; set; }
    public int Confidence { get; set; }
    public bool HasCheckins { get; set; }
    public bool IsOutdated { get; set; }
    public string? LastCheckinCollaboratorName { get; set; }
}
