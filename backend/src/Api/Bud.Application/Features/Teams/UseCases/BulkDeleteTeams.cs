using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Teams.UseCases;

public sealed record BulkDeleteTeamsCommand(List<Guid> Ids);

public sealed partial class BulkDeleteTeams(
    ITeamRepository teamRepository,
    ILogger<BulkDeleteTeams> logger)
{
    public async Task<Result> ExecuteAsync(
        BulkDeleteTeamsCommand command,
        CancellationToken cancellationToken = default)
    {
        LogBulkDeleting(logger, command.Ids.Count);

        if (command.Ids.Count == 0)
        {
            return Result.Success();
        }

        await teamRepository.BulkDeleteAsync(command.Ids, cancellationToken);

        LogBulkDeleted(logger, command.Ids.Count);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4053, Level = LogLevel.Information, Message = "Bulk deleting {Count} teams")]
    private static partial void LogBulkDeleting(ILogger logger, int count);

    [LoggerMessage(EventId = 4054, Level = LogLevel.Information, Message = "Bulk deleted {Count} teams successfully")]
    private static partial void LogBulkDeleted(ILogger logger, int count);
}
