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

public sealed class PatchCycle(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<PatchCycle> logger,
    IUnitOfWork? unitOfWork = null)
{
    private readonly ICycleRepository _cycleRepository = cycleRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly ILogger<PatchCycle> _logger = logger;
    private readonly IUnitOfWork? _unitOfWork = unitOfWork;

    public Task<Result<Cycle>> ExecuteAsync(
        Guid id,
        PatchCycleCommand command,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
