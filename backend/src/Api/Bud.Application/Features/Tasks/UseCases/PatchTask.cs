using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tasks.UseCases;

public sealed record PatchTaskCommand(
    Optional<string> Title,
    Optional<string?> Description,
    Optional<bool> IsDone,
    Optional<DateOnly?> DueDate);

public sealed partial class PatchTask(
    ITaskRepository taskRepository,
    ILogger<PatchTask> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<MissionTask>> ExecuteAsync(
        Guid id,
        PatchTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingTask(logger, id);

        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            LogTaskPatchFailed(logger, id, "Not found");
            return Result<MissionTask>.NotFound(UserErrorMessages.TaskNotFound);
        }

        try
        {
            var title = command.Title.HasValue ? (command.Title.Value ?? task.Title) : task.Title;
            var description = command.Description.HasValue ? command.Description.Value : task.Description;
            var dueDate = command.DueDate.HasValue ? command.DueDate.Value : task.DueDate;

            task.UpdateDetails(title, description, dueDate);

            if (command.IsDone.HasValue)
            {
                if (command.IsDone.Value) {
                    task.Complete();
                }
                else
                {
                    task.Reopen();
                }
            }

            await unitOfWork.CommitAsync(taskRepository.SaveChangesAsync, cancellationToken);

            LogTaskPatched(logger, id, task.Title);
            return Result<MissionTask>.Success(task);
        }
        catch (DomainInvariantException ex)
        {
            LogTaskPatchFailed(logger, id, ex.Message);
            return Result<MissionTask>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4083, Level = LogLevel.Information, Message = "Patching task {TaskId}")]
    private static partial void LogPatchingTask(ILogger logger, Guid taskId);

    [LoggerMessage(EventId = 4084, Level = LogLevel.Information, Message = "Task patched successfully: {TaskId} - '{Title}'")]
    private static partial void LogTaskPatched(ILogger logger, Guid taskId, string title);

    [LoggerMessage(EventId = 4085, Level = LogLevel.Warning, Message = "Task patch failed for {TaskId}: {Reason}")]
    private static partial void LogTaskPatchFailed(ILogger logger, Guid taskId, string reason);
}
