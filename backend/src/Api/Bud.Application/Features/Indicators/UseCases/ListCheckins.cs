using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed class ListCheckins(
    IIndicatorRepository indicatorRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<PagedResult<Checkin>>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid indicatorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);
        var canRead = await authorizationGateway.CanReadAsync(user, new IndicatorResource(indicatorId), cancellationToken);
        if (!canRead)
        {
            return Result<PagedResult<Checkin>>.Forbidden(UserErrorMessages.IndicatorNotFound);
        }

        var result = await indicatorRepository.GetCheckinsAsync(indicatorId, null, page, pageSize, cancellationToken);
        return Result<PagedResult<Checkin>>.Success(result.MapPaged(x => x));
    }
}
