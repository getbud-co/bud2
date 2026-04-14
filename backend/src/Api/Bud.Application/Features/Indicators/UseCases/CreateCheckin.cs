using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed record CreateCheckinCommand(
    decimal? Value,
    string? Text,
    DateTime CheckinDate,
    string? Note,
    int ConfidenceLevel);

public sealed partial class CreateCheckin(
    IIndicatorRepository indicatorRepository,
    IMemberRepository employeeRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateCheckin> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Checkin>> ExecuteAsync(
        Guid indicatorId,
        CreateCheckinCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingCheckin(logger, indicatorId);

        var indicator = await indicatorRepository.GetIndicatorWithMissionAsync(indicatorId, cancellationToken);
        if (indicator is null)
        {
            LogCheckinCreationFailed(logger, indicatorId, "Indicator not found");
            return Result<Checkin>.NotFound(UserErrorMessages.IndicatorNotFound);
        }

        var employeeId = tenantProvider.EmployeeId;
        if (!employeeId.HasValue)
        {
            LogCheckinCreationFailed(logger, indicatorId, "Employee not identified");
            return Result<Checkin>.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        var employee = await employeeRepository.GetByIdAsync(employeeId.Value, cancellationToken);
        if (employee is null)
        {
            LogCheckinCreationFailed(logger, indicatorId, "Employee not found");
            return Result<Checkin>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var mission = indicator.Mission;
        if (mission.Status != MissionStatus.Active)
        {
            LogCheckinCreationFailed(logger, indicatorId, "Mission not active");
            return Result<Checkin>.Failure(
                "Não é possível fazer check-in em indicadores de metas que não estão ativas.",
                ErrorType.Validation);
        }

        try
        {
            var checkin = indicator.CreateCheckin(
                Guid.NewGuid(),
                employeeId.Value,
                command.Value,
                command.Text,
                UtcDateTimeNormalizer.Normalize(command.CheckinDate),
                command.Note,
                command.ConfidenceLevel);

            await indicatorRepository.AddCheckinAsync(checkin, cancellationToken);
            await unitOfWork.CommitAsync(indicatorRepository.SaveChangesAsync, cancellationToken);

            LogCheckinCreated(logger, checkin.Id, indicatorId);
            return Result<Checkin>.Success(checkin);
        }
        catch (DomainInvariantException ex)
        {
            LogCheckinCreationFailed(logger, indicatorId, ex.Message);
            return Result<Checkin>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4060, Level = LogLevel.Information, Message = "Creating checkin for indicator {IndicatorId}")]
    private static partial void LogCreatingCheckin(ILogger logger, Guid indicatorId);

    [LoggerMessage(EventId = 4061, Level = LogLevel.Information, Message = "Checkin created successfully: {CheckinId} for indicator {IndicatorId}")]
    private static partial void LogCheckinCreated(ILogger logger, Guid checkinId, Guid indicatorId);

    [LoggerMessage(EventId = 4062, Level = LogLevel.Warning, Message = "Checkin creation failed for indicator {IndicatorId}: {Reason}")]
    private static partial void LogCheckinCreationFailed(ILogger logger, Guid indicatorId, string reason);
}
