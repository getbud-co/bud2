using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class ListAvailableEmployeesForTeam(
    ITeamRepository teamRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<List<TeamEmployeeEligibleResponse>>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid teamId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team is null)
        {
            return Result<List<TeamEmployeeEligibleResponse>>.NotFound(UserErrorMessages.TeamNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new TeamResource(teamId), cancellationToken);
        if (!canRead)
        {
            return Result<List<TeamEmployeeEligibleResponse>>.Forbidden(UserErrorMessages.TeamNotFound);
        }

        var summaries = await teamRepository.GetEligibleEmployeesForAssignmentAsync(teamId, team.OrganizationId, search, 50, cancellationToken);
        return Result<List<TeamEmployeeEligibleResponse>>.Success(summaries.Select(c => c.ToTeamEmployeeEligibleResponse()).ToList());
    }
}
