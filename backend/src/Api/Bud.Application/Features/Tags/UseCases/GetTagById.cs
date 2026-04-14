using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tags.UseCases;

public sealed partial class GetTagById(
    ITagRepository tagRepository,
    ILogger<GetTagById> logger)
{
    public async Task<Result<TagWithCount>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LogFetching(logger, id);

        var tag = await tagRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (tag is null)
        {
            LogNotFound(logger, id);
            return Result<TagWithCount>.NotFound(UserErrorMessages.TagNotFound);
        }

        var linkedItems = await tagRepository.GetLinkedItemsCountAsync(id, cancellationToken);
        return Result<TagWithCount>.Success(new TagWithCount(tag, linkedItems));
    }

    [LoggerMessage(EventId = 5009, Level = LogLevel.Information, Message = "Fetching tag {TagId}")]
    private static partial void LogFetching(ILogger logger, Guid tagId);

    [LoggerMessage(EventId = 5010, Level = LogLevel.Warning, Message = "Tag not found: {TagId}")]
    private static partial void LogNotFound(ILogger logger, Guid tagId);
}
