namespace Bud.Domain.Templates;

public readonly record struct TemplateMissionDraft(
    Guid? Id,
    Guid? ParentId,
    string Name,
    string? Description,
    int OrderIndex,
    string? Dimension);
