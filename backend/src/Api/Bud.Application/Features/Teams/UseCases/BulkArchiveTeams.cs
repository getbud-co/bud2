using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Teams.UseCases;

public sealed record BulkArchiveTeamsCommand(List<Guid> Ids);

public sealed partial class BulkArchiveTeams(
    ITeamRepository teamRepository,
    ILogger<BulkArchiveTeams> logger)
{
    public async Task<Result> ExecuteAsync(
        BulkArchiveTeamsCommand command,
        CancellationToken cancellationToken = default)
    {
        LogBulkArchiving(logger, command.Ids.Count);

        if (command.Ids.Count == 0)
        {
            return Result.Success();
        }

        await teamRepository.BulkUpdateStatusAsync(command.Ids, TeamStatus.Archived, cancellationToken);

        LogBulkArchived(logger, command.Ids.Count);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4050, Level = LogLevel.Information, Message = "Bulk archiving {Count} teams")]
    private static partial void LogBulkArchiving(ILogger logger, int count);

    [LoggerMessage(EventId = 4051, Level = LogLevel.Information, Message = "Bulk archived {Count} teams successfully")]
    private static partial void LogBulkArchived(ILogger logger, int count);
}
