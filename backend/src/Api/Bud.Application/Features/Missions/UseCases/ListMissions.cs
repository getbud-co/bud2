using Bud.Application.Common;

namespace Bud.Application.Features.Missions.UseCases;

public sealed class ListMissions(IMissionRepository missionRepository)
{
    public async Task<Result<PagedResult<Mission>>> ExecuteAsync(
        MissionFilter? filter,
        Guid? employeeId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var result = await missionRepository.GetAllAsync(
            filter,
            employeeId,
            search,
            page,
            pageSize,
            cancellationToken);

        return Result<PagedResult<Mission>>.Success(result.MapPaged(x => x));
    }
}
