using Bud.Application.Common;
using Bud.Application.Features.Missions;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;

namespace Bud.Infrastructure.Features.Missions;

public sealed class MissionAuthorizationService(
    IMissionRepository missionRepository,
    ITenantProvider tenantProvider)
    : IReadAuthorizationRule<MissionResource>,
      IWriteAuthorizationRule<MissionResource>,
      IWriteAuthorizationRule<CreateMissionContext>
{
    public async Task<Result> EvaluateAsync(MissionResource resource, CancellationToken cancellationToken = default)
    {
        return await TenantScopedAuthorization.AuthorizeReadAsync(
            tenantProvider,
            ct => missionRepository.GetByIdReadOnlyAsync(resource.MissionId, ct),
            mission => mission.OrganizationId,
            "Meta não encontrada.",
            "Você não tem permissão para acessar esta meta.",
            cancellationToken);
    }

    async Task<Result> IWriteAuthorizationRule<MissionResource>.EvaluateAsync(MissionResource resource, CancellationToken cancellationToken)
        => await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            ct => missionRepository.GetByIdReadOnlyAsync(resource.MissionId, ct),
            mission => mission.OrganizationId,
            "Meta não encontrada.",
            "Colaborador não identificado.",
            "Você não tem permissão para atualizar metas nesta organização.",
            cancellationToken);

    Task<Result> IWriteAuthorizationRule<CreateMissionContext>.EvaluateAsync(CreateMissionContext context, CancellationToken cancellationToken)
        => TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            context.OrganizationId,
            "Colaborador não identificado.",
            "Você não tem permissão para criar metas nesta organização.");
}
