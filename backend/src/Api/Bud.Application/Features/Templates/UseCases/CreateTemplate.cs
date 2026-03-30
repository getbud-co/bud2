using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Templates.UseCases;

public sealed record CreateTemplateCommand(
    string Name,
    string? Description,
    string? MissionNamePattern,
    string? MissionDescriptionPattern,
    IReadOnlyList<TemplateMissionDraft> Missions,
    IReadOnlyList<TemplateIndicatorDraft> Indicators);

public sealed partial class CreateTemplate(
    ITemplateRepository templateRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateTemplate> logger,
    IUnitOfWork? unitOfWork = null,
    IApplicationAuthorizationGateway? authorizationGateway = null)
{
    public Task<Result<Template>> ExecuteAsync(
        ClaimsPrincipal user,
        CreateTemplateCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsyncInternal(user, command, cancellationToken);

    public async Task<Result<Template>> ExecuteAsync(
        CreateTemplateCommand command,
        CancellationToken cancellationToken = default)
        => await ExecuteAsyncInternal(new ClaimsPrincipal(new ClaimsIdentity()), command, cancellationToken);

    private async Task<Result<Template>> ExecuteAsyncInternal(
        ClaimsPrincipal user,
        CreateTemplateCommand command,
        CancellationToken cancellationToken)
    {
        LogCreatingTemplate(logger, command.Name);

        if (tenantProvider.TenantId is null)
        {
            LogTemplateCreationFailed(logger, command.Name, "Tenant not selected");
            return Result<Template>.Forbidden(UserErrorMessages.TemplateCreateForbidden);
        }

        if (authorizationGateway is not null)
        {
            var canWrite = await authorizationGateway.CanWriteAsync(
                user,
                new CreateTemplateContext(tenantProvider.TenantId.Value),
                cancellationToken);
            if (!canWrite)
            {
                LogTemplateCreationFailed(logger, command.Name, UserErrorMessages.TemplateCreateForbidden);
                return Result<Template>.Forbidden(UserErrorMessages.TemplateCreateForbidden);
            }
        }

        try
        {
            var template = Template.Create(
                Guid.NewGuid(),
                Guid.Empty,
                command.Name,
                command.Description,
                command.MissionNamePattern,
                command.MissionDescriptionPattern);

            template.ReplaceMissionsAndIndicators(command.Missions, command.Indicators);

            await templateRepository.AddAsync(template, cancellationToken);
            await unitOfWork.CommitAsync(templateRepository.SaveChangesAsync, cancellationToken);

            LogTemplateCreated(logger, template.Id, template.Name);
            return Result<Template>.Success(template);
        }
        catch (DomainInvariantException ex)
        {
            LogTemplateCreationFailed(logger, command.Name, ex.Message);
            return Result<Template>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4070, Level = LogLevel.Information, Message = "Creating template '{Name}'")]
    private static partial void LogCreatingTemplate(ILogger logger, string name);

    [LoggerMessage(EventId = 4071, Level = LogLevel.Information, Message = "Template created successfully: {TemplateId} - '{Name}'")]
    private static partial void LogTemplateCreated(ILogger logger, Guid templateId, string name);

    [LoggerMessage(EventId = 4072, Level = LogLevel.Warning, Message = "Template creation failed for '{Name}': {Reason}")]
    private static partial void LogTemplateCreationFailed(ILogger logger, string name, string reason);
}
