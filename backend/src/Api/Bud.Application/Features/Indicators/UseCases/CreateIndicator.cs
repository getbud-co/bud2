using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed record CreateIndicatorCommand(
    Guid MissionId,
    Guid EmployeeId,
    string Title,
    IndicatorMeasurementMode MeasurementMode,
    IndicatorGoalType GoalType,
    decimal StartValue,
    decimal? TargetValue,
    decimal? LowThreshold,
    decimal? HighThreshold,
    IndicatorUnit Unit,
    string UnitLabel,
    string SortOrder,
    Guid? ParentKrId = null);

public sealed partial class CreateIndicator(
    IIndicatorRepository indicatorRepository,
    ILogger<CreateIndicator> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Indicator>> ExecuteAsync(
        CreateIndicatorCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingIndicator(logger, command.Title, command.MissionId);

        var mission = await indicatorRepository.GetMissionByIdAsync(command.MissionId, cancellationToken);
        if (mission is null)
        {
            LogIndicatorCreationFailed(logger, command.Title, UserErrorMessages.MissionNotFound);
            return Result<Indicator>.NotFound(UserErrorMessages.MissionNotFound);
        }

        try
        {
            var indicator = Indicator.Create(
                Guid.NewGuid(),
                mission.OrganizationId,
                command.MissionId,
                command.EmployeeId,
                command.Title,
                command.MeasurementMode,
                command.GoalType,
                command.StartValue,
                command.TargetValue,
                command.LowThreshold,
                command.HighThreshold,
                command.Unit,
                command.UnitLabel,
                command.SortOrder,
                command.ParentKrId);

            await indicatorRepository.AddAsync(indicator, cancellationToken);
            await unitOfWork.CommitAsync(indicatorRepository.SaveChangesAsync, cancellationToken);

            LogIndicatorCreated(logger, indicator.Id, indicator.Title);
            return Result<Indicator>.Success(indicator);
        }
        catch (DomainInvariantException ex)
        {
            LogIndicatorCreationFailed(logger, command.Title, ex.Message);
            return Result<Indicator>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4050, Level = LogLevel.Information, Message = "Creating indicator '{Title}' for mission {MissionId}")]
    private static partial void LogCreatingIndicator(ILogger logger, string title, Guid missionId);

    [LoggerMessage(EventId = 4051, Level = LogLevel.Information, Message = "Indicator created successfully: {IndicatorId} - '{Title}'")]
    private static partial void LogIndicatorCreated(ILogger logger, Guid indicatorId, string title);

    [LoggerMessage(EventId = 4052, Level = LogLevel.Warning, Message = "Indicator creation failed for '{Title}': {Reason}")]
    private static partial void LogIndicatorCreationFailed(ILogger logger, string title, string reason);
}
