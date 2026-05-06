using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed record PatchCheckinCommand(
    decimal Value,
    DateTime CheckinDate,
    string? Note,
    int ConfidenceLevel);

public sealed partial class PatchCheckin(
    IIndicatorRepository indicatorRepository,
    ITenantProvider tenantProvider,
    ILogger<PatchCheckin> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Checkin>> ExecuteAsync(
        Guid indicatorId,
        Guid checkinId,
        PatchCheckinCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingCheckin(logger, checkinId, indicatorId);

        var checkin = await indicatorRepository.GetCheckinByIdForUpdateAsync(checkinId, cancellationToken);
        if (checkin is null || checkin.IndicatorId != indicatorId)
        {
            LogCheckinPatchFailed(logger, checkinId, "Not found");
            return Result<Checkin>.NotFound(UserErrorMessages.CheckinNotFound);
        }

        var employeeId = tenantProvider.EmployeeId;
        if (!employeeId.HasValue)
        {
            LogCheckinPatchFailed(logger, checkinId, "Employee not identified");
            return Result<Checkin>.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        if (checkin.EmployeeId != employeeId.Value)
        {
            LogCheckinPatchFailed(logger, checkinId, "Author mismatch");
            return Result<Checkin>.Forbidden(UserErrorMessages.CheckinEditAuthorOnly);
        }

        var indicator = await indicatorRepository.GetByIdAsync(indicatorId, cancellationToken);
        if (indicator is null)
        {
            LogCheckinPatchFailed(logger, checkinId, "Indicator not found");
            return Result<Checkin>.NotFound(UserErrorMessages.IndicatorNotFound);
        }

        try
        {
            indicator.UpdateCheckin(
                checkin,
                command.Value,
                UtcDateTimeNormalizer.Normalize(command.CheckinDate),
                command.Note,
                command.ConfidenceLevel);

            await unitOfWork.CommitAsync(indicatorRepository.SaveChangesAsync, cancellationToken);
            LogCheckinPatched(logger, checkinId, indicatorId);
            return Result<Checkin>.Success(checkin);
        }
        catch (DomainInvariantException ex)
        {
            LogCheckinPatchFailed(logger, checkinId, ex.Message);
            return Result<Checkin>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4063, Level = LogLevel.Information, Message = "Patching checkin {CheckinId} for indicator {IndicatorId}")]
    private static partial void LogPatchingCheckin(ILogger logger, Guid checkinId, Guid indicatorId);

    [LoggerMessage(EventId = 4064, Level = LogLevel.Information, Message = "Checkin patched successfully: {CheckinId} for indicator {IndicatorId}")]
    private static partial void LogCheckinPatched(ILogger logger, Guid checkinId, Guid indicatorId);

    [LoggerMessage(EventId = 4065, Level = LogLevel.Warning, Message = "Checkin patch failed for {CheckinId}: {Reason}")]
    private static partial void LogCheckinPatchFailed(ILogger logger, Guid checkinId, string reason);
}
