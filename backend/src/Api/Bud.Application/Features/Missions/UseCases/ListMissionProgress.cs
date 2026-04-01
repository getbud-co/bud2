using Bud.Application.Common;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Missions.UseCases;

public sealed class ListMissionProgress(IMissionProgressReadStore missionProgressReadStore)
{
    public async Task<Result<List<MissionProgressResponse>>> ExecuteAsync(
        List<Guid> missionIds,
        CancellationToken cancellationToken = default)
    {
        var result = await missionProgressReadStore.GetProgressAsync(missionIds, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<List<MissionProgressResponse>>.Failure(
                result.Error ?? UserErrorMessages.MissionProgressCalculationFailed,
                result.ErrorType);
        }

        return Result<List<MissionProgressResponse>>.Success(
            result.Value!.Select(p => p.ToResponse()).ToList());
    }
}
