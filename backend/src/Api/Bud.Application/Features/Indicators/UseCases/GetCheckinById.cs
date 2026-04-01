using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed class GetCheckinById(
    IIndicatorRepository indicatorRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<Checkin>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid indicatorId,
        Guid checkinId,
        CancellationToken cancellationToken = default)
    {
        var checkin = await indicatorRepository.GetCheckinByIdAsync(checkinId, cancellationToken);
        if (checkin is null || checkin.IndicatorId != indicatorId)
        {
            return Result<Checkin>.NotFound(UserErrorMessages.CheckinNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new IndicatorResource(indicatorId), cancellationToken);
        if (!canRead)
        {
            return Result<Checkin>.Forbidden(UserErrorMessages.CheckinNotFound);
        }

        return Result<Checkin>.Success(checkin);
    }
}
