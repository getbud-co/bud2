using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Missions.UseCases;

public sealed class ListMissionIndicators(
    IMissionRepository missionRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<PagedResult<Indicator>>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid missionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var missionExists = await missionRepository.ExistsAsync(missionId, cancellationToken);
        if (!missionExists)
        {
            return Result<PagedResult<Indicator>>.NotFound(UserErrorMessages.MissionNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new MissionResource(missionId), cancellationToken);
        if (!canRead)
        {
            return Result<PagedResult<Indicator>>.Forbidden(UserErrorMessages.MissionNotFound);
        }

        var result = await missionRepository.GetIndicatorsAsync(missionId, page, pageSize, cancellationToken);
        return Result<PagedResult<Indicator>>.Success(result.MapPaged(x => x));
    }
}
