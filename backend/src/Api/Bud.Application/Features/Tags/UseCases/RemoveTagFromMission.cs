using Bud.Application.Common;
using Bud.Application.Features.Missions;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tags.UseCases;

public sealed partial class RemoveTagFromMission(
    ITagRepository tagRepository,
    IMissionRepository missionRepository,
    ILogger<RemoveTagFromMission> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        Guid missionId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        LogRemoving(logger, missionId, tagId);

        var mission = await missionRepository.GetByIdReadOnlyAsync(missionId, cancellationToken);
        if (mission is null)
        {
            LogRemoveFailed(logger, missionId, tagId, "Mission not found");
            return Result.NotFound(UserErrorMessages.MissionNotFound);
        }

        var missionTag = await tagRepository.GetMissionTagAsync(missionId, tagId, cancellationToken);
        if (missionTag is null)
        {
            return Result.Success();
        }

        await tagRepository.RemoveMissionTagAsync(missionTag, cancellationToken);
        await unitOfWork.CommitAsync(tagRepository.SaveChangesAsync, cancellationToken);

        LogRemoved(logger, missionId, tagId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 5015, Level = LogLevel.Information, Message = "Removing tag {TagId} from mission {MissionId}")]
    private static partial void LogRemoving(ILogger logger, Guid missionId, Guid tagId);

    [LoggerMessage(EventId = 5016, Level = LogLevel.Information, Message = "Tag {TagId} removed from mission {MissionId}")]
    private static partial void LogRemoved(ILogger logger, Guid missionId, Guid tagId);

    [LoggerMessage(EventId = 5017, Level = LogLevel.Warning, Message = "Tag removal failed for mission {MissionId} / tag {TagId}: {Reason}")]
    private static partial void LogRemoveFailed(ILogger logger, Guid missionId, Guid tagId, string reason);
}
