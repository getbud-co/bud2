using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Missions.UseCases;

public sealed class ListMissionChildren(IMissionRepository missionRepository)
{
    public async Task<Result<PagedResult<Mission>>> ExecuteAsync(
        Guid parentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var parentExists = await missionRepository.ExistsAsync(parentId, cancellationToken);
        if (!parentExists)
        {
            return Result<PagedResult<Mission>>.NotFound(UserErrorMessages.MissionNotFound);
        }

        var result = await missionRepository.GetChildrenAsync(parentId, page, pageSize, cancellationToken);
        return Result<PagedResult<Mission>>.Success(result.MapPaged(x => x));
    }
}
