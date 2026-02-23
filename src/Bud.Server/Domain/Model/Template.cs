namespace Bud.Server.Domain.Model;

public sealed class Template : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MissionNamePattern { get; set; }
    public string? MissionDescriptionPattern { get; set; }

    public ICollection<TemplateObjective> Objectives { get; set; } = new List<TemplateObjective>();
    public ICollection<TemplateMetric> Metrics { get; set; } = new List<TemplateMetric>();

    public static Template Create(
        Guid id,
        Guid organizationId,
        string name,
        string? description,
        string? missionNamePattern,
        string? missionDescriptionPattern)
    {
        var template = new Template
        {
            Id = id,
            OrganizationId = organizationId,
        };

        template.UpdateBasics(name, description, missionNamePattern, missionDescriptionPattern);
        return template;
    }

    public void UpdateBasics(
        string name,
        string? description,
        string? missionNamePattern,
        string? missionDescriptionPattern)
    {
        if (!EntityName.TryCreate(name, out var entityName))
        {
            throw new DomainInvariantException("O nome do template de missão é obrigatório e deve ter até 200 caracteres.");
        }

        Name = entityName.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        MissionNamePattern = string.IsNullOrWhiteSpace(missionNamePattern) ? null : missionNamePattern.Trim();
        MissionDescriptionPattern = string.IsNullOrWhiteSpace(missionDescriptionPattern) ? null : missionDescriptionPattern.Trim();
    }

    public void ReplaceMetrics(IEnumerable<TemplateMetricDraft> metricDrafts)
        => ReplaceObjectivesAndMetrics([], metricDrafts);

    public void ReplaceObjectivesAndMetrics(
        IEnumerable<TemplateObjectiveDraft> objectiveDrafts,
        IEnumerable<TemplateMetricDraft> metricDrafts)
    {
        ArgumentNullException.ThrowIfNull(objectiveDrafts);
        ArgumentNullException.ThrowIfNull(metricDrafts);

        var objectiveList = objectiveDrafts
            .Select(objective => TemplateObjective.Create(
                objective.Id ?? Guid.NewGuid(),
                OrganizationId,
                Id,
                objective.Name,
                objective.Description,
                objective.OrderIndex,
                objective.Dimension))
            .ToList();

        var objectiveIds = objectiveList
            .Select(objective => objective.Id)
            .ToHashSet();

        Objectives = objectiveList;

        Metrics = metricDrafts
            .Select(metric =>
            {
                if (metric.TemplateObjectiveId.HasValue && !objectiveIds.Contains(metric.TemplateObjectiveId.Value))
                {
                    throw new DomainInvariantException("A métrica referencia um objetivo de template inexistente.");
                }

                return TemplateMetric.Create(
                    Guid.NewGuid(),
                    OrganizationId,
                    Id,
                    metric.Name,
                    metric.Type,
                    metric.OrderIndex,
                    metric.TemplateObjectiveId,
                    metric.QuantitativeType,
                    metric.MinValue,
                    metric.MaxValue,
                    metric.Unit,
                    metric.TargetText);
            })
            .ToList();
    }
}
