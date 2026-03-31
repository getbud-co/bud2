using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tasks.UseCases;

public sealed record PatchTaskCommand(
    Optional<string> Name,
    Optional<string?> Description,
    Optional<TaskState> State,
    Optional<DateTime?> DueDate);

public sealed partial class PatchTask(
    ITaskRepository taskRepository,
    ILogger<PatchTask> logger,
    IApplicationAuthorizationGateway authorizationGateway,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<MissionTask>> ExecuteAsync(
        ClaimsPrincipal user,
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

        var canWrite = await authorizationGateway.CanWriteAsync(user, new TaskResource(id), cancellationToken);
        if (!canWrite)
        {
            LogTaskPatchFailed(logger, id, UserErrorMessages.TaskUpdateForbidden);
            return Result<MissionTask>.Forbidden(UserErrorMessages.TaskUpdateForbidden);
        }

        try
        {
            var name = command.Name.HasValue ? (command.Name.Value ?? task.Name) : task.Name;
            var description = command.Description.HasValue ? command.Description.Value : task.Description;
            var state = command.State.HasValue ? command.State.Value : task.State;
            var dueDate = command.DueDate.HasValue ? command.DueDate.Value : task.DueDate;

            task.UpdateDetails(name, description, state, dueDate);
            await unitOfWork.CommitAsync(taskRepository.SaveChangesAsync, cancellationToken);

            LogTaskPatched(logger, id, task.Name);
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

    [LoggerMessage(EventId = 4084, Level = LogLevel.Information, Message = "Task patched successfully: {TaskId} - '{Name}'")]
    private static partial void LogTaskPatched(ILogger logger, Guid taskId, string name);

    [LoggerMessage(EventId = 4085, Level = LogLevel.Warning, Message = "Task patch failed for {TaskId}: {Reason}")]
    private static partial void LogTaskPatchFailed(ILogger logger, Guid taskId, string reason);
}
