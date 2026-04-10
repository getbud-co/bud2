using Bud.Application.Common;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class ListAvailableEmployeesForTeam(
    ITeamRepository teamRepository)
{
    public async Task<Result<List<TeamEmployeeEligibleResponse>>> ExecuteAsync(
        Guid teamId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team is null)
        {
            return Result<List<TeamEmployeeEligibleResponse>>.NotFound(UserErrorMessages.TeamNotFound);
        }

        var summaries = await teamRepository.GetEligibleEmployeesForAssignmentAsync(teamId, team.OrganizationId, search, 50, cancellationToken);
        return Result<List<TeamEmployeeEligibleResponse>>.Success(summaries.Select(c => c.ToTeamEmployeeEligibleResponse()).ToList());
    }
}
