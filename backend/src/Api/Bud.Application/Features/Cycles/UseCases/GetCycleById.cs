using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed partial class GetCycleById(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<GetCycleById> logger)
{
    public async Task<Result<Cycle>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var cycle = await cycleRepository.GetByIdAsync(id, cancellationToken);
        if (cycle is null)
        {
            LogNotFound(logger, id);
            return Result<Cycle>.NotFound(UserErrorMessages.CycleNotFound);
        }

        if (cycle.OrganizationId != tenantProvider.TenantId)
        {
            LogNotFound(logger, id);
            return Result<Cycle>.NotFound(UserErrorMessages.CycleNotFound);
        }

        return Result<Cycle>.Success(cycle);
    }

    [LoggerMessage(EventId = 4102, Level = LogLevel.Warning, Message = "Cycle {CycleId} not found")]
    private static partial void LogNotFound(ILogger logger, Guid cycleId);
}
