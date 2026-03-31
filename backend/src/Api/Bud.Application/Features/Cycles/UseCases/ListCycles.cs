using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed class ListCycles(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<ListCycles> logger)
{
    private readonly ICycleRepository _cycleRepository = cycleRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly ILogger<ListCycles> _logger = logger;

    public Task<Result<List<Cycle>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
