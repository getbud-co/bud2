namespace Bud.Application.Features.Templates;

public static class TemplateContractMapper
{
    public static TemplateResponse ToResponse(this Template source)
    {
        return new TemplateResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            Name = source.Name,
            Description = source.Description,
            MissionNamePattern = source.MissionNamePattern,
            MissionDescriptionPattern = source.MissionDescriptionPattern,
            Missions = source.Missions.Select(g => g.ToResponse(source.Indicators)).ToList(),
            Indicators = source.Indicators.Select(i => i.ToResponse()).ToList()
        };
    }

    public static TemplateMissionResponse ToResponse(this TemplateMission source, IEnumerable<TemplateIndicator> allIndicators)
    {
        return new TemplateMissionResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            TemplateId = source.TemplateId,
            ParentId = source.ParentId,
            Name = source.Name,
            Description = source.Description,
            OrderIndex = source.OrderIndex,
            Dimension = source.Dimension,
            Indicators = allIndicators
                .Where(i => i.TemplateMissionId == source.Id)
                .Select(i => i.ToResponse())
                .ToList()
        };
    }

    public static TemplateIndicatorResponse ToResponse(this TemplateIndicator source)
    {
        return new TemplateIndicatorResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            TemplateId = source.TemplateId,
            TemplateMissionId = source.TemplateMissionId,
            Name = source.Name,
            Type = source.Type,
            OrderIndex = source.OrderIndex,
            QuantitativeType = source.QuantitativeType,
            MinValue = source.MinValue,
            MaxValue = source.MaxValue,
            Unit = source.Unit,
            TargetText = source.TargetText
        };
    }
}
