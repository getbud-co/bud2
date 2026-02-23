using Bud.Server.Application.Common;
using Bud.Server.Application.Mapping;
using Bud.Server.Domain.Model;

namespace Bud.Server.Application.UseCases.Missions;

public sealed class ListMissionsByScope(IMissionRepository missionRepository)
{
    public async Task<Result<PagedResult<Mission>>> ExecuteAsync(
        MissionScopeType? scopeType,
        Guid? scopeId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var result = await missionRepository.GetAllAsync(
            scopeType,
            scopeId,
            search,
            page,
            pageSize,
            cancellationToken);

        return Result<PagedResult<Mission>>.Success(result.MapPaged(x => x));
    }
}
