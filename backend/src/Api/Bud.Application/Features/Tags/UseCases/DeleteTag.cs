using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tags.UseCases;

public sealed partial class DeleteTag(
    ITagRepository tagRepository,
    ILogger<DeleteTag> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LogDeleting(logger, id);

        var tag = await tagRepository.GetByIdAsync(id, cancellationToken);
        if (tag is null)
        {
            LogDeletionFailed(logger, id, "Not found");
            return Result.NotFound(UserErrorMessages.TagNotFound);
        }

        await tagRepository.RemoveAsync(tag, cancellationToken);
        await unitOfWork.CommitAsync(tagRepository.SaveChangesAsync, cancellationToken);

        LogDeleted(logger, id);
        return Result.Success();
    }

    [LoggerMessage(EventId = 5006, Level = LogLevel.Information, Message = "Deleting tag {TagId}")]
    private static partial void LogDeleting(ILogger logger, Guid tagId);

    [LoggerMessage(EventId = 5007, Level = LogLevel.Information, Message = "Tag deleted successfully: {TagId}")]
    private static partial void LogDeleted(ILogger logger, Guid tagId);

    [LoggerMessage(EventId = 5008, Level = LogLevel.Warning, Message = "Tag deletion failed for {TagId}: {Reason}")]
    private static partial void LogDeletionFailed(ILogger logger, Guid tagId, string reason);
}
