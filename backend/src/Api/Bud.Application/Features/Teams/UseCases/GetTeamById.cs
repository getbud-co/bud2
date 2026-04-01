using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class GetTeamById(
    ITeamRepository teamRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<Team>> ExecuteAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetByIdAsync(id, cancellationToken);
        if (team is null)
        {
            return Result<Team>.NotFound(UserErrorMessages.TeamNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new TeamResource(id), cancellationToken);
        if (!canRead)
        {
            return Result<Team>.Forbidden(UserErrorMessages.TeamNotFound);
        }

        return Result<Team>.Success(team);
    }
}
