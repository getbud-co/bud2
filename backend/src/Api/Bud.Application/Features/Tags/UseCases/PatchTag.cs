using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tags.UseCases;

public sealed record PatchTagCommand(string Name, TagColor Color);

public sealed partial class PatchTag(
    ITagRepository tagRepository,
    ILogger<PatchTag> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Tag>> ExecuteAsync(
        Guid id,
        PatchTagCommand command,
        CancellationToken cancellationToken = default)
    {
        LogUpdating(logger, id);

        var tag = await tagRepository.GetByIdAsync(id, cancellationToken);
        if (tag is null)
        {
            LogUpdateFailed(logger, id, "Not found");
            return Result<Tag>.NotFound(UserErrorMessages.TagNotFound);
        }

        var isNameUnique = await tagRepository.IsNameUniqueAsync(command.Name, tag.OrganizationId, id, cancellationToken);
        if (!isNameUnique)
        {
            LogUpdateFailed(logger, id, "Name already in use");
            return Result<Tag>.Failure(UserErrorMessages.TagNameConflict, ErrorType.Conflict);
        }

        try
        {
            tag.UpdateDetails(command.Name, command.Color);
            await unitOfWork.CommitAsync(tagRepository.SaveChangesAsync, cancellationToken);

            LogUpdated(logger, id);
            return Result<Tag>.Success(tag);
        }
        catch (DomainInvariantException ex)
        {
            LogUpdateFailed(logger, id, ex.Message);
            return Result<Tag>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 5003, Level = LogLevel.Information, Message = "Updating tag {TagId}")]
    private static partial void LogUpdating(ILogger logger, Guid tagId);

    [LoggerMessage(EventId = 5004, Level = LogLevel.Information, Message = "Tag updated successfully: {TagId}")]
    private static partial void LogUpdated(ILogger logger, Guid tagId);

    [LoggerMessage(EventId = 5005, Level = LogLevel.Warning, Message = "Tag update failed for {TagId}: {Reason}")]
    private static partial void LogUpdateFailed(ILogger logger, Guid tagId, string reason);
}
