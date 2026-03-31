using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed class DeleteCycle(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<DeleteCycle> logger,
    IUnitOfWork? unitOfWork = null)
{
    private readonly ICycleRepository _cycleRepository = cycleRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly ILogger<DeleteCycle> _logger = logger;
    private readonly IUnitOfWork? _unitOfWork = unitOfWork;

    public Task<Result> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
