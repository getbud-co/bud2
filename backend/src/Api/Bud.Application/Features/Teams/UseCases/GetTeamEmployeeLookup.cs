using Bud.Application.Common;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class GetTeamEmployeeLookup(
    ITeamRepository teamRepository)
{
    public async Task<Result<List<EmployeeLookupResponse>>> ExecuteAsync(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        if (!await teamRepository.ExistsAsync(teamId, cancellationToken))
        {
            return Result<List<EmployeeLookupResponse>>.NotFound(UserErrorMessages.TeamNotFound);
        }

        var summaries = await teamRepository.GetEmployeeLookupAsync(teamId, cancellationToken);
        return Result<List<EmployeeLookupResponse>>.Success(summaries.Select(c => c.ToLookupResponse()).ToList());
    }
}
