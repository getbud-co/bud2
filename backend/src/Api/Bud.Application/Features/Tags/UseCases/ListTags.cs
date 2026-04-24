using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tags.UseCases;

public sealed partial class ListTags(
    ITagRepository tagRepository,
    ILogger<ListTags> logger)
{
    public async Task<Result<List<TagWithCount>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        LogListing(logger);

        var tags = await tagRepository.GetAllWithCountsAsync(cancellationToken);
        return Result<List<TagWithCount>>.Success(tags);
    }

    [LoggerMessage(EventId = 5011, Level = LogLevel.Information, Message = "Listing tags")]
    private static partial void LogListing(ILogger logger);
}
