using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Missions.UseCases;

public sealed class GetMissionById(
    IMissionRepository missionRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<Mission>> ExecuteAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await missionRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (mission is null)
        {
            return Result<Mission>.NotFound(UserErrorMessages.MissionNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new MissionResource(id), cancellationToken);
        if (!canRead)
        {
            return Result<Mission>.Forbidden(UserErrorMessages.MissionNotFound);
        }

        return Result<Mission>.Success(mission);
    }
}
