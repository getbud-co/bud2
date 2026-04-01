using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed partial class DeleteCheckin(
    IIndicatorRepository indicatorRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ITenantProvider tenantProvider,
    ILogger<DeleteCheckin> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        ClaimsPrincipal user,
        Guid indicatorId,
        Guid checkinId,
        CancellationToken cancellationToken = default)
    {
        LogDeletingCheckin(logger, checkinId, indicatorId);

        var checkin = await indicatorRepository.GetCheckinByIdAsync(checkinId, cancellationToken);
        if (checkin is null || checkin.IndicatorId != indicatorId)
        {
            LogCheckinDeletionFailed(logger, checkinId, "Not found");
            return Result.NotFound(UserErrorMessages.CheckinNotFound);
        }

        var canWrite = await authorizationGateway.CanWriteAsync(user, new IndicatorResource(indicatorId), cancellationToken);
        if (!canWrite)
        {
            LogCheckinDeletionFailed(logger, checkinId, "Indicator write forbidden");
            return Result.Forbidden(UserErrorMessages.CheckinDeleteForbidden);
        }

        var employeeId = tenantProvider.EmployeeId;
        if (!employeeId.HasValue)
        {
            LogCheckinDeletionFailed(logger, checkinId, "Employee not identified");
            return Result.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        if (checkin.EmployeeId != employeeId.Value)
        {
            LogCheckinDeletionFailed(logger, checkinId, "Author mismatch");
            return Result.Forbidden(UserErrorMessages.CheckinDeleteAuthorOnly);
        }

        await indicatorRepository.RemoveCheckinAsync(checkin, cancellationToken);
        await unitOfWork.CommitAsync(indicatorRepository.SaveChangesAsync, cancellationToken);

        LogCheckinDeleted(logger, checkinId, indicatorId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4066, Level = LogLevel.Information, Message = "Deleting checkin {CheckinId} for indicator {IndicatorId}")]
    private static partial void LogDeletingCheckin(ILogger logger, Guid checkinId, Guid indicatorId);

    [LoggerMessage(EventId = 4067, Level = LogLevel.Information, Message = "Checkin deleted successfully: {CheckinId} for indicator {IndicatorId}")]
    private static partial void LogCheckinDeleted(ILogger logger, Guid checkinId, Guid indicatorId);

    [LoggerMessage(EventId = 4068, Level = LogLevel.Warning, Message = "Checkin deletion failed for {CheckinId}: {Reason}")]
    private static partial void LogCheckinDeletionFailed(ILogger logger, Guid checkinId, string reason);
}
