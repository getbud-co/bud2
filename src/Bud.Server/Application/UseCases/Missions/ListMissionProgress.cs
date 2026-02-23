using Bud.Server.Application.Common;
using Bud.Server.Application.Mapping;
using Bud.Server.Application.ReadModels;
using Bud.Server.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Server.Application.UseCases.Missions;

public sealed class ListMissionProgress(IMissionProgressService missionProgressService)
{
    public async Task<Result<List<MissionProgressResponse>>> ExecuteAsync(
        List<Guid> missionIds,
        CancellationToken cancellationToken = default)
    {
        var result = await missionProgressService.GetProgressAsync(missionIds, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<List<MissionProgressResponse>>.Failure(
                result.Error ?? "Falha ao calcular progresso das missões.",
                result.ErrorType);
        }

        return Result<List<MissionProgressResponse>>.Success(
            result.Value!.Select(p => p.ToResponse()).ToList());
    }
}
