using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Templates.UseCases;

public sealed record PatchTemplateCommand(
    Optional<string> Name,
    Optional<string?> Description,
    Optional<string?> MissionNamePattern,
    Optional<string?> MissionDescriptionPattern,
    IReadOnlyList<TemplateMissionDraft> Missions,
    IReadOnlyList<TemplateIndicatorDraft> Indicators);

public sealed partial class PatchTemplate(
    ITemplateRepository templateRepository,
    ILogger<PatchTemplate> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Template>> ExecuteAsync(
        Guid id,
        PatchTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingTemplate(logger, id);

        var template = await templateRepository.GetByIdWithChildrenAsync(id, cancellationToken);
        if (template is null)
        {
            LogTemplatePatchFailed(logger, id, "Not found");
            return Result<Template>.NotFound(UserErrorMessages.TemplateNotFound);
        }

        try
        {
            template.UpdateBasics(
                command.Name.HasValue ? (command.Name.Value ?? template.Name) : template.Name,
                command.Description.HasValue ? command.Description.Value : template.Description,
                command.MissionNamePattern.HasValue ? command.MissionNamePattern.Value : template.MissionNamePattern,
                command.MissionDescriptionPattern.HasValue ? command.MissionDescriptionPattern.Value : template.MissionDescriptionPattern);

            var previousIndicators = template.Indicators.ToList();
            var previousMissions = template.Missions.ToList();

            template.ReplaceMissionsAndIndicators(command.Missions, command.Indicators);

            await templateRepository.RemoveMissionsAndIndicatorsAsync(previousMissions, previousIndicators, cancellationToken);
            await templateRepository.AddMissionsAndIndicatorsAsync(template.Missions, template.Indicators, cancellationToken);
            await unitOfWork.CommitAsync(templateRepository.SaveChangesAsync, cancellationToken);

            var reloadedTemplate = await templateRepository.GetByIdReadOnlyAsync(id, cancellationToken);
            LogTemplatePatched(logger, id, template.Name);
            return Result<Template>.Success(reloadedTemplate!);
        }
        catch (DomainInvariantException ex)
        {
            LogTemplatePatchFailed(logger, id, ex.Message);
            return Result<Template>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4073, Level = LogLevel.Information, Message = "Patching template {TemplateId}")]
    private static partial void LogPatchingTemplate(ILogger logger, Guid templateId);

    [LoggerMessage(EventId = 4074, Level = LogLevel.Information, Message = "Template patched successfully: {TemplateId} - '{Name}'")]
    private static partial void LogTemplatePatched(ILogger logger, Guid templateId, string name);

    [LoggerMessage(EventId = 4075, Level = LogLevel.Warning, Message = "Template patch failed for {TemplateId}: {Reason}")]
    private static partial void LogTemplatePatchFailed(ILogger logger, Guid templateId, string reason);
}
