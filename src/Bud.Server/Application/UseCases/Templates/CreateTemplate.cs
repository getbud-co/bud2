using System.Security.Claims;
using Bud.Server.Application.Common;
using Bud.Server.Application.Mapping;
using Bud.Server.Authorization;
using Bud.Server.Domain.Model;
using Bud.Server.Domain.Repositories;
using Bud.Server.MultiTenancy;
using Bud.Shared.Contracts;

namespace Bud.Server.Application.UseCases.Templates;

public sealed class CreateTemplate(
    ITemplateRepository templateRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ITenantProvider tenantProvider,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Template>> ExecuteAsync(
        ClaimsPrincipal user,
        CreateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (tenantProvider.TenantId.HasValue)
        {
            var canCreate = await authorizationGateway.CanAccessTenantOrganizationAsync(
                user,
                tenantProvider.TenantId.Value,
                cancellationToken);
            if (!canCreate)
            {
                return Result<Template>.Forbidden("Você não tem permissão para criar templates nesta organização.");
            }
        }

        try
        {
            var template = Template.Create(
                Guid.NewGuid(),
                Guid.Empty,
                request.Name,
                request.Description,
                request.MissionNamePattern,
                request.MissionDescriptionPattern);

            template.ReplaceObjectivesAndMetrics(
                request.Objectives.Select(objective => new TemplateObjectiveDraft(
                    objective.Id,
                    objective.Name,
                    objective.Description,
                    objective.OrderIndex,
                    objective.Dimension)),
                request.Metrics.Select(metric => new TemplateMetricDraft(
                    metric.Name,
                    metric.Type,
                    metric.OrderIndex,
                    metric.TemplateObjectiveId,
                    metric.QuantitativeType,
                    metric.MinValue,
                    metric.MaxValue,
                    metric.Unit,
                    metric.TargetText)));

            await templateRepository.AddAsync(template, cancellationToken);
            await unitOfWork.CommitAsync(templateRepository.SaveChangesAsync, cancellationToken);

            return Result<Template>.Success(template);
        }
        catch (DomainInvariantException ex)
        {
            return Result<Template>.Failure(ex.Message, ErrorType.Validation);
        }
    }
}

