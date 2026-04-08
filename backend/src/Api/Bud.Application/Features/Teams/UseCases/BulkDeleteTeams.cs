using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Teams.UseCases;

public sealed record BulkDeleteTeamsCommand(List<Guid> Ids);

public sealed partial class BulkDeleteTeams(
    ITeamRepository teamRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ILogger<BulkDeleteTeams> logger)
{
    public async Task<Result> ExecuteAsync(
        ClaimsPrincipal user,
        BulkDeleteTeamsCommand command,
        CancellationToken cancellationToken = default)
    {
        LogBulkDeleting(logger, command.Ids.Count);

        if (command.Ids.Count == 0)
        {
            return Result.Success();
        }

        foreach (var id in command.Ids)
        {
            var canWrite = await authorizationGateway.CanWriteAsync(user, new TeamResource(id), cancellationToken);
            if (!canWrite)
            {
                LogBulkDeleteFailed(logger, id, "Forbidden");
                return Result.Forbidden(UserErrorMessages.TeamUpdateForbidden);
            }
        }

        await teamRepository.BulkDeleteAsync(command.Ids, cancellationToken);

        LogBulkDeleted(logger, command.Ids.Count);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4053, Level = LogLevel.Information, Message = "Bulk deleting {Count} teams")]
    private static partial void LogBulkDeleting(ILogger logger, int count);

    [LoggerMessage(EventId = 4054, Level = LogLevel.Information, Message = "Bulk deleted {Count} teams successfully")]
    private static partial void LogBulkDeleted(ILogger logger, int count);

    [LoggerMessage(EventId = 4055, Level = LogLevel.Warning, Message = "Bulk delete failed for team {TeamId}: {Reason}")]
    private static partial void LogBulkDeleteFailed(ILogger logger, Guid teamId, string reason);
}
