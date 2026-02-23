namespace Bud.Server.Domain.Model;

public readonly record struct TemplateMetricDraft(
    string Name,
    MetricType Type,
    int OrderIndex,
    Guid? TemplateObjectiveId,
    QuantitativeMetricType? QuantitativeType,
    decimal? MinValue,
    decimal? MaxValue,
    MetricUnit? Unit,
    string? TargetText);
