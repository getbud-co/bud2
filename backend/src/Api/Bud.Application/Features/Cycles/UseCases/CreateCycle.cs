using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed record CreateCycleCommand(
    string Name,
    CycleCadence Cadence,
    DateTime StartDate,
    DateTime EndDate,
    CycleStatus Status);

public sealed partial class CreateCycle(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateCycle> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Cycle>> ExecuteAsync(
        CreateCycleCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreating(logger, command.Name);

        var organizationId = tenantProvider.TenantId;
        if (!organizationId.HasValue)
        {
            LogCreationFailed(logger, command.Name, "Tenant not selected");
            return Result<Cycle>.Forbidden(UserErrorMessages.CycleCreateForbidden);
        }

        try
        {
            var cycle = Cycle.Create(
                Guid.NewGuid(),
                organizationId.Value,
                command.Name,
                command.Cadence,
                UtcDateTimeNormalizer.Normalize(command.StartDate),
                UtcDateTimeNormalizer.Normalize(command.EndDate),
                command.Status,
                tenantProvider.EmployeeId);

            await cycleRepository.AddAsync(cycle, cancellationToken);
            await unitOfWork.CommitAsync(cycleRepository.SaveChangesAsync, cancellationToken);

            LogCreated(logger, cycle.Id, cycle.Name);
            return Result<Cycle>.Success(cycle);
        }
        catch (DomainInvariantException ex)
        {
            LogCreationFailed(logger, command.Name, ex.Message);
            return Result<Cycle>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4103, Level = LogLevel.Information, Message = "Creating cycle '{Name}'")]
    private static partial void LogCreating(ILogger logger, string name);

    [LoggerMessage(EventId = 4104, Level = LogLevel.Information, Message = "Cycle created successfully: {CycleId} - '{Name}'")]
    private static partial void LogCreated(ILogger logger, Guid cycleId, string name);

    [LoggerMessage(EventId = 4105, Level = LogLevel.Warning, Message = "Cycle creation failed for '{Name}': {Reason}")]
    private static partial void LogCreationFailed(ILogger logger, string name, string reason);
}
