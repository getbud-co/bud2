using Bud.Application.Common;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class GetTeamById(
    ITeamRepository teamRepository)
{
    public async Task<Result<Team>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetByIdAsync(id, cancellationToken);
        if (team is null)
        {
            return Result<Team>.NotFound(UserErrorMessages.TeamNotFound);
        }

        return Result<Team>.Success(team);
    }
}
