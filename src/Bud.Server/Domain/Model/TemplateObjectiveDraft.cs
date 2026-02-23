namespace Bud.Server.Domain.Model;

public readonly record struct TemplateObjectiveDraft(
    Guid? Id,
    string Name,
    string? Description,
    int OrderIndex,
    string? Dimension);
