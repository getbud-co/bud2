using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Cycles.UseCases;

public sealed partial class ListCycles(
    ICycleRepository cycleRepository,
    ITenantProvider tenantProvider,
    ILogger<ListCycles> logger)
{
    public async Task<Result<List<Cycle>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var organizationId = tenantProvider.TenantId;
        if (!organizationId.HasValue)
        {
            LogListFailed(logger, "Tenant not selected");
            return Result<List<Cycle>>.Forbidden(UserErrorMessages.CycleListForbidden);
        }

        var cycles = await cycleRepository.GetAllAsync(organizationId.Value, cancellationToken);

        LogListed(logger, cycles.Count, organizationId.Value);
        return Result<List<Cycle>>.Success(cycles);
    }

    [LoggerMessage(EventId = 4100, Level = LogLevel.Information, Message = "Listed {Count} cycles for organization {OrganizationId}")]
    private static partial void LogListed(ILogger logger, int count, Guid organizationId);

    [LoggerMessage(EventId = 4101, Level = LogLevel.Warning, Message = "List cycles failed: {Reason}")]
    private static partial void LogListFailed(ILogger logger, string reason);
}
