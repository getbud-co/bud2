using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Missions.UseCases;

public sealed partial class DeleteMission(
    IMissionRepository missionRepository,
    ITenantProvider tenantProvider,
    ILogger<DeleteMission> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LogDeletingMission(logger, id);

        var mission = await missionRepository.GetByIdAsync(id, cancellationToken);
        if (mission is null)
        {
            LogMissionDeletionFailed(logger, id, "Not found");
            return Result.NotFound(UserErrorMessages.MissionNotFound);
        }

        mission.MarkAsDeleted(tenantProvider.EmployeeId);
        await missionRepository.RemoveAsync(mission, cancellationToken);
        await unitOfWork.CommitAsync(missionRepository.SaveChangesAsync, cancellationToken);

        LogMissionDeleted(logger, id);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4006, Level = LogLevel.Information, Message = "Deleting mission {MissionId}")]
    private static partial void LogDeletingMission(ILogger logger, Guid missionId);

    [LoggerMessage(EventId = 4007, Level = LogLevel.Information, Message = "Mission deleted successfully: {MissionId}")]
    private static partial void LogMissionDeleted(ILogger logger, Guid missionId);

    [LoggerMessage(EventId = 4008, Level = LogLevel.Warning, Message = "Mission deletion failed for {MissionId}: {Reason}")]
    private static partial void LogMissionDeletionFailed(ILogger logger, Guid missionId, string reason);
}
