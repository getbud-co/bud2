using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tasks.UseCases;

public sealed record CreateTaskCommand(
    Guid MissionId,
    string Name,
    string? Description,
    TaskState State,
    DateTime? DueDate);

public sealed partial class CreateTask(
    ITaskRepository taskRepository,
    ILogger<CreateTask> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<MissionTask>> ExecuteAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingTask(logger, command.Name, command.MissionId);

        var mission = await taskRepository.GetMissionByIdAsync(command.MissionId, cancellationToken);
        if (mission is null)
        {
            LogTaskCreationFailed(logger, command.Name, UserErrorMessages.MissionNotFound);
            return Result<MissionTask>.NotFound(UserErrorMessages.MissionNotFound);
        }

        try
        {
            var task = MissionTask.Create(
                Guid.NewGuid(),
                mission.OrganizationId,
                command.MissionId,
                command.Name,
                command.Description,
                command.State,
                command.DueDate);

            await taskRepository.AddAsync(task, cancellationToken);
            await unitOfWork.CommitAsync(taskRepository.SaveChangesAsync, cancellationToken);

            LogTaskCreated(logger, task.Id, task.Name);
            return Result<MissionTask>.Success(task);
        }
        catch (DomainInvariantException ex)
        {
            LogTaskCreationFailed(logger, command.Name, ex.Message);
            return Result<MissionTask>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4080, Level = LogLevel.Information, Message = "Creating task '{Name}' for mission {MissionId}")]
    private static partial void LogCreatingTask(ILogger logger, string name, Guid missionId);

    [LoggerMessage(EventId = 4081, Level = LogLevel.Information, Message = "Task created successfully: {TaskId} - '{Name}'")]
    private static partial void LogTaskCreated(ILogger logger, Guid taskId, string name);

    [LoggerMessage(EventId = 4082, Level = LogLevel.Warning, Message = "Task creation failed for '{Name}': {Reason}")]
    private static partial void LogTaskCreationFailed(ILogger logger, string name, string reason);
}
