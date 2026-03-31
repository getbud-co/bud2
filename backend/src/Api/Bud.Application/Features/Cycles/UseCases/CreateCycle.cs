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

public sealed class CreateCycle(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateCycle> logger,
    IUnitOfWork? unitOfWork = null)
{
    private readonly ICycleRepository _cycleRepository = cycleRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly ILogger<CreateCycle> _logger = logger;
    private readonly IUnitOfWork? _unitOfWork = unitOfWork;

    public Task<Result<Cycle>> ExecuteAsync(
        CreateCycleCommand command,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
