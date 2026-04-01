using Bud.Application.Common;
using Bud.Application.Features.Indicators;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;

namespace Bud.Infrastructure.Features.Indicators;

public sealed class IndicatorAuthorizationService(
    IIndicatorRepository indicatorRepository,
    ITenantProvider tenantProvider)
    : IReadAuthorizationRule<IndicatorResource>,
      IWriteAuthorizationRule<IndicatorResource>,
      IWriteAuthorizationRule<CreateIndicatorContext>,
      IWriteAuthorizationRule<CreateCheckinContext>
{
    public async Task<Result> EvaluateAsync(IndicatorResource resource, CancellationToken cancellationToken = default)
        => await TenantScopedAuthorization.AuthorizeReadAsync(
            tenantProvider,
            ct => indicatorRepository.GetByIdAsync(resource.IndicatorId, ct),
            indicator => indicator.OrganizationId,
            "Indicador não encontrado.",
            "Você não tem permissão para acessar este indicador.",
            cancellationToken);

    async Task<Result> IWriteAuthorizationRule<IndicatorResource>.EvaluateAsync(IndicatorResource resource, CancellationToken cancellationToken)
        => await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            ct => indicatorRepository.GetByIdAsync(resource.IndicatorId, ct),
            indicator => indicator.OrganizationId,
            "Indicador não encontrado.",
            "Colaborador não identificado.",
            "Você não tem permissão para atualizar indicadores nesta meta.",
            cancellationToken);

    async Task<Result> IWriteAuthorizationRule<CreateIndicatorContext>.EvaluateAsync(CreateIndicatorContext context, CancellationToken cancellationToken)
    {
        var mission = await indicatorRepository.GetMissionByIdAsync(context.MissionId, cancellationToken);
        if (mission is null)
        {
            return Result.NotFound("Meta não encontrada.");
        }

        return await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            mission.OrganizationId,
            "Funcionário não identificado.",
            "Você não tem permissão para criar indicadores nesta meta.");
    }

    async Task<Result> IWriteAuthorizationRule<CreateCheckinContext>.EvaluateAsync(CreateCheckinContext context, CancellationToken cancellationToken)
    {
        var indicator = await indicatorRepository.GetByIdAsync(context.IndicatorId, cancellationToken);
        if (indicator is null)
        {
            return Result.NotFound("Indicador não encontrado.");
        }

        return await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            indicator.OrganizationId,
            "Funcionário não identificado.",
            "Você não tem permissão para criar check-ins neste indicador.");
    }
}
