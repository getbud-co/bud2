using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed record PatchCycleCommand(
    Optional<string> Name,
    Optional<CycleCadence> Cadence,
    Optional<DateTime> StartDate,
    Optional<DateTime> EndDate,
    Optional<CycleStatus> Status);

public sealed partial class PatchCycle(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<PatchCycle> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Cycle>> ExecuteAsync(
        Guid id,
        PatchCycleCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatching(logger, id);

        var cycle = await cycleRepository.GetByIdAsync(id, cancellationToken);
        if (cycle is null)
        {
            LogPatchFailed(logger, id, "Not found");
            return Result<Cycle>.NotFound(UserErrorMessages.CycleNotFound);
        }

        try
        {
            var name = command.Name.HasValue ? (command.Name.Value ?? cycle.Name) : cycle.Name;
            var cadence = command.Cadence.HasValue ? command.Cadence.Value : cycle.Cadence;
            var startDate = command.StartDate.HasValue ? command.StartDate.Value : cycle.StartDate;
            var endDate = command.EndDate.HasValue ? command.EndDate.Value : cycle.EndDate;
            var status = command.Status.HasValue ? command.Status.Value : cycle.Status;

            cycle.UpdateDetails(
                name,
                cadence,
                UtcDateTimeNormalizer.Normalize(startDate),
                UtcDateTimeNormalizer.Normalize(endDate),
                status);

            cycle.MarkAsUpdated(tenantProvider.CollaboratorId);
            await unitOfWork.CommitAsync(cycleRepository.SaveChangesAsync, cancellationToken);

            LogPatched(logger, id, cycle.Name);
            return Result<Cycle>.Success(cycle);
        }
        catch (DomainInvariantException ex)
        {
            LogPatchFailed(logger, id, ex.Message);
            return Result<Cycle>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4106, Level = LogLevel.Information, Message = "Patching cycle {CycleId}")]
    private static partial void LogPatching(ILogger logger, Guid cycleId);

    [LoggerMessage(EventId = 4107, Level = LogLevel.Information, Message = "Cycle patched successfully: {CycleId} - '{Name}'")]
    private static partial void LogPatched(ILogger logger, Guid cycleId, string name);

    [LoggerMessage(EventId = 4108, Level = LogLevel.Warning, Message = "Cycle patch failed for {CycleId}: {Reason}")]
    private static partial void LogPatchFailed(ILogger logger, Guid cycleId, string reason);
}
