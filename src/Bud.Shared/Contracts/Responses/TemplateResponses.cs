using Bud.Shared.Contracts;

namespace Bud.Shared.Contracts.Responses;

public sealed class TemplateResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MissionNamePattern { get; set; }
    public string? MissionDescriptionPattern { get; set; }
    public List<TemplateObjectiveResponse> Objectives { get; set; } = [];
    public List<TemplateMetricResponse> Metrics { get; set; } = [];
}

public sealed class TemplateObjectiveResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string? Dimension { get; set; }
    public List<TemplateMetricResponse> Metrics { get; set; } = [];
}

public sealed class TemplateMetricResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid? TemplateObjectiveId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MetricType Type { get; set; }
    public int OrderIndex { get; set; }
    public QuantitativeMetricType? QuantitativeType { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public MetricUnit? Unit { get; set; }
    public string? TargetText { get; set; }
    public TemplateObjectiveResponse? Objective { get; set; }
}
