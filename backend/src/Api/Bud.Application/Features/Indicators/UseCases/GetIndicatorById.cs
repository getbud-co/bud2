using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed class GetIndicatorById(
    IIndicatorRepository indicatorRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<Indicator>> ExecuteAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken = default)
    {
        var indicator = await indicatorRepository.GetByIdAsync(id, cancellationToken);
        if (indicator is null)
        {
            return Result<Indicator>.NotFound(UserErrorMessages.IndicatorNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new IndicatorResource(id), cancellationToken);
        if (!canRead)
        {
            return Result<Indicator>.Forbidden(UserErrorMessages.IndicatorNotFound);
        }

        return Result<Indicator>.Success(indicator);
    }
}
