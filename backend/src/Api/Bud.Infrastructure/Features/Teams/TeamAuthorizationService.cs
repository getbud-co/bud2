using Bud.Application.Common;
using Bud.Application.Features.Teams;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;
using Bud.Infrastructure.Persistence;

namespace Bud.Infrastructure.Features.Teams;

public sealed class TeamAuthorizationService(
    ITeamRepository teamRepository,
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider)
    : IReadAuthorizationRule<TeamResource>,
      IWriteAuthorizationRule<TeamResource>,
      IWriteAuthorizationRule<CreateTeamContext>
{
    public async Task<Result> EvaluateAsync(TeamResource resource, CancellationToken cancellationToken = default)
        => await TenantScopedAuthorization.AuthorizeReadAsync(
            tenantProvider,
            ct => teamRepository.GetByIdAsync(resource.TeamId, ct),
            team => team.OrganizationId,
            "Time não encontrado.",
            "Você não tem permissão para acessar este time.",
            cancellationToken);

    async Task<Result> IWriteAuthorizationRule<TeamResource>.EvaluateAsync(TeamResource resource, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdAsync(resource.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.NotFound("Time não encontrado.");
        }

        return await RequireLeaderAccessAsync(team.OrganizationId, cancellationToken);
    }

    async Task<Result> IWriteAuthorizationRule<CreateTeamContext>.EvaluateAsync(CreateTeamContext context, CancellationToken cancellationToken)
        => await RequireLeaderAccessAsync(context.OrganizationId, cancellationToken);

    private Task<Result> RequireLeaderAccessAsync(Guid organizationId, CancellationToken cancellationToken)
        => LeaderScopedAuthorization.RequireLeaderInOrganizationAsync(
            dbContext,
            tenantProvider,
            organizationId,
            "Colaborador não identificado.",
            "Apenas um líder da organização pode realizar esta ação.",
            cancellationToken);
}
