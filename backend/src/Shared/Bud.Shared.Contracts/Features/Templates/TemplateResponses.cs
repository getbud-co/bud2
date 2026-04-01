using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Templates;

public sealed class TemplateResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MissionNamePattern { get; set; }
    public string? MissionDescriptionPattern { get; set; }
    public List<TemplateMissionResponse> Missions { get; set; } = [];
    public List<TemplateIndicatorResponse> Indicators { get; set; } = [];
}

public sealed class TemplateMissionResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string? Dimension { get; set; }
    public List<TemplateIndicatorResponse> Indicators { get; set; } = [];
}

public sealed class TemplateIndicatorResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid? TemplateMissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IndicatorType Type { get; set; }
    public int OrderIndex { get; set; }
    public QuantitativeIndicatorType? QuantitativeType { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public IndicatorUnit? Unit { get; set; }
    public string? TargetText { get; set; }
    public TemplateMissionResponse? Mission { get; set; }
}
