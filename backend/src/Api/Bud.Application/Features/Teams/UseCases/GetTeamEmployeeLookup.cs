using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class GetTeamEmployeeLookup(
    ITeamRepository teamRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<List<EmployeeLookupResponse>>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        if (!await teamRepository.ExistsAsync(teamId, cancellationToken))
        {
            return Result<List<EmployeeLookupResponse>>.NotFound(UserErrorMessages.TeamNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new TeamResource(teamId), cancellationToken);
        if (!canRead)
        {
            return Result<List<EmployeeLookupResponse>>.Forbidden(UserErrorMessages.TeamNotFound);
        }

        var summaries = await teamRepository.GetEmployeeLookupAsync(teamId, cancellationToken);
        return Result<List<EmployeeLookupResponse>>.Success(summaries.Select(c => c.ToResponse()).ToList());
    }
}
