using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed partial class DeleteCycle(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<DeleteCycle> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LogDeleting(logger, id);

        var cycle = await cycleRepository.GetByIdAsync(id, cancellationToken);
        if (cycle is null)
        {
            LogDeletionFailed(logger, id, "Not found");
            return Result.NotFound(UserErrorMessages.CycleNotFound);
        }

        cycle.MarkAsDeleted(tenantProvider.EmployeeId);
        await cycleRepository.RemoveAsync(cycle, cancellationToken);
        await unitOfWork.CommitAsync(cycleRepository.SaveChangesAsync, cancellationToken);

        LogDeleted(logger, id);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4109, Level = LogLevel.Information, Message = "Deleting cycle {CycleId}")]
    private static partial void LogDeleting(ILogger logger, Guid cycleId);

    [LoggerMessage(EventId = 4110, Level = LogLevel.Information, Message = "Cycle deleted successfully: {CycleId}")]
    private static partial void LogDeleted(ILogger logger, Guid cycleId);

    [LoggerMessage(EventId = 4111, Level = LogLevel.Warning, Message = "Cycle deletion failed for {CycleId}: {Reason}")]
    private static partial void LogDeletionFailed(ILogger logger, Guid cycleId, string reason);
}
