using Bud.Application.Common;
using Bud.Application.Features.Indicators;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;

namespace Bud.Infrastructure.Features.Indicators;

public sealed class IndicatorAuthorizationService(
    IIndicatorRepository indicatorRepository,
    ITenantProvider tenantProvider)
    : IReadAuthorizationRule<IndicatorResource>,
      IWriteAuthorizationRule<IndicatorResource>
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
}
