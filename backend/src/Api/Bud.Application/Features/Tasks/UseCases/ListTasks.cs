using Bud.Application.Common;
using Bud.Application.Features.Missions;
using Bud.Application.Ports;

namespace Bud.Application.Features.Tasks.UseCases;

public sealed class ListTasks(
    ITaskRepository taskRepository,
    IMissionRepository missionRepository)
{
    public async Task<Result<PagedResult<MissionTask>>> ExecuteAsync(
        Guid missionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var missionExists = await missionRepository.ExistsAsync(missionId, cancellationToken);
        if (!missionExists)
        {
            return Result<PagedResult<MissionTask>>.NotFound(UserErrorMessages.MissionNotFound);
        }

        var result = await taskRepository.GetByMissionIdAsync(missionId, page, pageSize, cancellationToken);
        return Result<PagedResult<MissionTask>>.Success(result);
    }
}
