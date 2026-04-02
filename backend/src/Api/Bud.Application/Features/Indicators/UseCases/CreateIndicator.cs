using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Indicators.UseCases;

public sealed record CreateIndicatorCommand(
    Guid MissionId,
    string Name,
    IndicatorType Type,
    QuantitativeIndicatorType? QuantitativeType,
    decimal? MinValue,
    decimal? MaxValue,
    IndicatorUnit? Unit,
    string? TargetText);

public sealed partial class CreateIndicator(
    IIndicatorRepository indicatorRepository,
    ILogger<CreateIndicator> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Indicator>> ExecuteAsync(
        CreateIndicatorCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingIndicator(logger, command.Name, command.MissionId);

        var mission = await indicatorRepository.GetMissionByIdAsync(command.MissionId, cancellationToken);
        if (mission is null)
        {
            LogIndicatorCreationFailed(logger, command.Name, UserErrorMessages.MissionNotFound);
            return Result<Indicator>.NotFound(UserErrorMessages.MissionNotFound);
        }

        try
        {
            var indicator = Indicator.Create(
                Guid.NewGuid(),
                mission.OrganizationId,
                command.MissionId,
                command.Name,
                command.Type);

            indicator.ApplyTarget(command.Type, command.QuantitativeType, command.MinValue, command.MaxValue, command.Unit, command.TargetText);

            await indicatorRepository.AddAsync(indicator, cancellationToken);
            await unitOfWork.CommitAsync(indicatorRepository.SaveChangesAsync, cancellationToken);

            LogIndicatorCreated(logger, indicator.Id, indicator.Name);
            return Result<Indicator>.Success(indicator);
        }
        catch (DomainInvariantException ex)
        {
            LogIndicatorCreationFailed(logger, command.Name, ex.Message);
            return Result<Indicator>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4050, Level = LogLevel.Information, Message = "Creating indicator '{Name}' for mission {MissionId}")]
    private static partial void LogCreatingIndicator(ILogger logger, string name, Guid missionId);

    [LoggerMessage(EventId = 4051, Level = LogLevel.Information, Message = "Indicator created successfully: {IndicatorId} - '{Name}'")]
    private static partial void LogIndicatorCreated(ILogger logger, Guid indicatorId, string name);

    [LoggerMessage(EventId = 4052, Level = LogLevel.Warning, Message = "Indicator creation failed for '{Name}': {Reason}")]
    private static partial void LogIndicatorCreationFailed(ILogger logger, string name, string reason);
}
