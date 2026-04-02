using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Missions.UseCases;

public sealed class GetMissionById(IMissionRepository missionRepository)
{
    public async Task<Result<Mission>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await missionRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        return mission is null
            ? Result<Mission>.NotFound(UserErrorMessages.MissionNotFound)
            : Result<Mission>.Success(mission);
    }
}
