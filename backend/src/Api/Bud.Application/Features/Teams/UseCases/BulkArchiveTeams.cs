using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Teams.UseCases;

public sealed record BulkArchiveTeamsCommand(List<Guid> Ids);

public sealed partial class BulkArchiveTeams(
    ITeamRepository teamRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ILogger<BulkArchiveTeams> logger)
{
    public async Task<Result> ExecuteAsync(
        ClaimsPrincipal user,
        BulkArchiveTeamsCommand command,
        CancellationToken cancellationToken = default)
    {
        LogBulkArchiving(logger, command.Ids.Count);

        if (command.Ids.Count == 0)
        {
            return Result.Success();
        }

        foreach (var id in command.Ids)
        {
            var canWrite = await authorizationGateway.CanWriteAsync(user, new TeamResource(id), cancellationToken);
            if (!canWrite)
            {
                LogBulkArchiveFailed(logger, id, "Forbidden");
                return Result.Forbidden(UserErrorMessages.TeamUpdateForbidden);
            }
        }

        await teamRepository.BulkUpdateStatusAsync(command.Ids, TeamStatus.Archived, cancellationToken);

        LogBulkArchived(logger, command.Ids.Count);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4050, Level = LogLevel.Information, Message = "Bulk archiving {Count} teams")]
    private static partial void LogBulkArchiving(ILogger logger, int count);

    [LoggerMessage(EventId = 4051, Level = LogLevel.Information, Message = "Bulk archived {Count} teams successfully")]
    private static partial void LogBulkArchived(ILogger logger, int count);

    [LoggerMessage(EventId = 4052, Level = LogLevel.Warning, Message = "Bulk archive failed for team {TeamId}: {Reason}")]
    private static partial void LogBulkArchiveFailed(ILogger logger, Guid teamId, string reason);
}
