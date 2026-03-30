namespace Bud.Domain.Templates;

public readonly record struct TemplateIndicatorDraft(
    string Name,
    IndicatorType Type,
    int OrderIndex,
    Guid? TemplateMissionId,
    QuantitativeIndicatorType? QuantitativeType,
    decimal? MinValue,
    decimal? MaxValue,
    IndicatorUnit? Unit,
    string? TargetText);
