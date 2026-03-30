namespace Bud.Domain.Templates;

public sealed class Template : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MissionNamePattern { get; set; }
    public string? MissionDescriptionPattern { get; set; }

    public ICollection<TemplateMission> Missions { get; set; } = new List<TemplateMission>();
    public ICollection<TemplateIndicator> Indicators { get; set; } = new List<TemplateIndicator>();

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
            throw new DomainInvariantException("O nome do template de meta é obrigatório e deve ter até 200 caracteres.");
        }

        Name = entityName.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        MissionNamePattern = string.IsNullOrWhiteSpace(missionNamePattern) ? null : missionNamePattern.Trim();
        MissionDescriptionPattern = string.IsNullOrWhiteSpace(missionDescriptionPattern) ? null : missionDescriptionPattern.Trim();
    }

    public void ReplaceIndicators(IEnumerable<TemplateIndicatorDraft> indicatorDrafts)
        => ReplaceMissionsAndIndicators([], indicatorDrafts);

    public void ReplaceMissionsAndIndicators(
        IEnumerable<TemplateMissionDraft> missionDrafts,
        IEnumerable<TemplateIndicatorDraft> indicatorDrafts)
    {
        ArgumentNullException.ThrowIfNull(missionDrafts);
        ArgumentNullException.ThrowIfNull(indicatorDrafts);

        var missionList = missionDrafts
            .Select(mission => TemplateMission.Create(
                mission.Id ?? Guid.NewGuid(),
                OrganizationId,
                Id,
                mission.Name,
                mission.Description,
                mission.OrderIndex,
                mission.Dimension,
                mission.ParentId))
            .ToList();

        var missionIds = missionList
            .Select(mission => mission.Id)
            .ToHashSet();

        Missions = missionList;

        Indicators = indicatorDrafts
            .Select(indicator =>
            {
                if (indicator.TemplateMissionId.HasValue && !missionIds.Contains(indicator.TemplateMissionId.Value))
                {
                    throw new DomainInvariantException("O indicador referencia uma meta de template inexistente.");
                }

                return TemplateIndicator.Create(
                    Guid.NewGuid(),
                    OrganizationId,
                    Id,
                    indicator.Name,
                    indicator.Type,
                    indicator.OrderIndex,
                    indicator.TemplateMissionId,
                    indicator.QuantitativeType,
                    indicator.MinValue,
                    indicator.MaxValue,
                    indicator.Unit,
                    indicator.TargetText);
            })
            .ToList();
    }
}
