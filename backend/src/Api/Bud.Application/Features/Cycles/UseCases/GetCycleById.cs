using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed class GetCycleById(
    ICycleRepository cycleRepository,
    ILogger<GetCycleById> logger)
{
    private readonly ICycleRepository _cycleRepository = cycleRepository;
    private readonly ILogger<GetCycleById> _logger = logger;

    public Task<Result<Cycle>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
