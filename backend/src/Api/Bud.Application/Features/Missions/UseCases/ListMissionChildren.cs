using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Missions.UseCases;

public sealed class ListMissionChildren(
    IMissionRepository missionRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<PagedResult<Mission>>> ExecuteAsync(
        ClaimsPrincipal user,
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

        var canRead = await authorizationGateway.CanReadAsync(user, new MissionResource(parentId), cancellationToken);
        if (!canRead)
        {
            return Result<PagedResult<Mission>>.Forbidden(UserErrorMessages.MissionNotFound);
        }

        var result = await missionRepository.GetChildrenAsync(parentId, page, pageSize, cancellationToken);
        return Result<PagedResult<Mission>>.Success(result.MapPaged(x => x));
    }
}
