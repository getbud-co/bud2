using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed record PatchIndicatorCommand(
    Optional<string> Title,
    Optional<string?> Description,
    Optional<Guid> EmployeeId,
    Optional<IndicatorMeasurementMode> MeasurementMode,
    Optional<IndicatorGoalType> GoalType,
    Optional<decimal> StartValue,
    Optional<decimal?> TargetValue,
    Optional<decimal?> LowThreshold,
    Optional<decimal?> HighThreshold,
    Optional<IndicatorUnit> Unit,
    Optional<string> UnitLabel,
    Optional<decimal?> ExpectedValue,
    Optional<string?> PeriodLabel,
    Optional<DateOnly?> PeriodStart,
    Optional<DateOnly?> PeriodEnd,
    Optional<Guid?> LinkedMissionId,
    Optional<Guid?> LinkedSurveyId,
    Optional<IndicatorExternalSource?> ExternalSource,
    Optional<string?> ExternalConfig);

public sealed partial class PatchIndicator(
    IIndicatorRepository indicatorRepository,
    ILogger<PatchIndicator> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Indicator>> ExecuteAsync(
        Guid id,
        PatchIndicatorCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingIndicator(logger, id);

        var indicator = await indicatorRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (indicator is null)
        {
            LogIndicatorPatchFailed(logger, id, "Not found");
            return Result<Indicator>.NotFound(UserErrorMessages.IndicatorNotFound);
        }

        try
        {
            var title = command.Title.HasValue ? (command.Title.Value ?? indicator.Title) : indicator.Title;
            var description = command.Description.HasValue ? command.Description.Value : indicator.Description;
            var employeeId = command.EmployeeId.HasValue ? command.EmployeeId.Value : indicator.EmployeeId;
            var measurementMode = command.MeasurementMode.HasValue ? command.MeasurementMode.Value : indicator.MeasurementMode;
            var goalType = command.GoalType.HasValue ? command.GoalType.Value : indicator.GoalType;
            var startValue = command.StartValue.HasValue ? command.StartValue.Value : indicator.StartValue;
            var targetValue = command.TargetValue.HasValue ? command.TargetValue.Value : indicator.TargetValue;
            var lowThreshold = command.LowThreshold.HasValue ? command.LowThreshold.Value : indicator.LowThreshold;
            var highThreshold = command.HighThreshold.HasValue ? command.HighThreshold.Value : indicator.HighThreshold;
            var unit = command.Unit.HasValue ? command.Unit.Value : indicator.Unit;
            var unitLabel = command.UnitLabel.HasValue ? (command.UnitLabel.Value ?? indicator.UnitLabel) : indicator.UnitLabel;
            var expectedValue = command.ExpectedValue.HasValue ? command.ExpectedValue.Value : indicator.ExpectedValue;
            var periodLabel = command.PeriodLabel.HasValue ? command.PeriodLabel.Value : indicator.PeriodLabel;
            var periodStart = command.PeriodStart.HasValue ? command.PeriodStart.Value : indicator.PeriodStart;
            var periodEnd = command.PeriodEnd.HasValue ? command.PeriodEnd.Value : indicator.PeriodEnd;
            var linkedMissionId = command.LinkedMissionId.HasValue ? command.LinkedMissionId.Value : indicator.LinkedMissionId;
            var linkedSurveyId = command.LinkedSurveyId.HasValue ? command.LinkedSurveyId.Value : indicator.LinkedSurveyId;
            var externalSource = command.ExternalSource.HasValue ? command.ExternalSource.Value : indicator.ExternalSource;
            var externalConfig = command.ExternalConfig.HasValue ? command.ExternalConfig.Value : indicator.ExternalConfig;

            indicator.UpdateDetails(
                title, description, employeeId,
                measurementMode, goalType,
                startValue, targetValue, lowThreshold, highThreshold,
                unit, unitLabel, expectedValue,
                periodLabel, periodStart, periodEnd,
                linkedMissionId, linkedSurveyId,
                externalSource, externalConfig);

            await unitOfWork.CommitAsync(indicatorRepository.SaveChangesAsync, cancellationToken);

            LogIndicatorPatched(logger, id, indicator.Title);
            return Result<Indicator>.Success(indicator);
        }
        catch (DomainInvariantException ex)
        {
            LogIndicatorPatchFailed(logger, id, ex.Message);
            return Result<Indicator>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4053, Level = LogLevel.Information, Message = "Patching indicator {IndicatorId}")]
    private static partial void LogPatchingIndicator(ILogger logger, Guid indicatorId);

    [LoggerMessage(EventId = 4054, Level = LogLevel.Information, Message = "Indicator patched successfully: {IndicatorId} - '{Title}'")]
    private static partial void LogIndicatorPatched(ILogger logger, Guid indicatorId, string title);

    [LoggerMessage(EventId = 4055, Level = LogLevel.Warning, Message = "Indicator patch failed for {IndicatorId}: {Reason}")]
    private static partial void LogIndicatorPatchFailed(ILogger logger, Guid indicatorId, string reason);
}
