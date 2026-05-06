using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tasks.UseCases;

public sealed record CreateTaskCommand(
    Guid MissionId,
    Guid EmployeeId,
    string Title,
    string? Description,
    string SortOrder,
    DateOnly? DueDate);

public sealed partial class CreateTask(
    ITaskRepository taskRepository,
    ILogger<CreateTask> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<MissionTask>> ExecuteAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingTask(logger, command.Title, command.MissionId);

        var mission = await taskRepository.GetMissionByIdAsync(command.MissionId, cancellationToken);
        if (mission is null)
        {
            LogTaskCreationFailed(logger, command.Title, UserErrorMessages.MissionNotFound);
            return Result<MissionTask>.NotFound(UserErrorMessages.MissionNotFound);
        }

        try
        {
            var task = MissionTask.Create(
                Guid.NewGuid(),
                mission.OrganizationId,
                command.MissionId,
                command.EmployeeId,
                command.Title,
                command.SortOrder,
                command.Description,
                command.DueDate);

            await taskRepository.AddAsync(task, cancellationToken);
            await unitOfWork.CommitAsync(taskRepository.SaveChangesAsync, cancellationToken);

            LogTaskCreated(logger, task.Id, task.Title);
            return Result<MissionTask>.Success(task);
        }
        catch (DomainInvariantException ex)
        {
            LogTaskCreationFailed(logger, command.Title, ex.Message);
            return Result<MissionTask>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4080, Level = LogLevel.Information, Message = "Creating task '{Title}' for mission {MissionId}")]
    private static partial void LogCreatingTask(ILogger logger, string title, Guid missionId);

    [LoggerMessage(EventId = 4081, Level = LogLevel.Information, Message = "Task created successfully: {TaskId} - '{Title}'")]
    private static partial void LogTaskCreated(ILogger logger, Guid taskId, string title);

    [LoggerMessage(EventId = 4082, Level = LogLevel.Warning, Message = "Task creation failed for '{Title}': {Reason}")]
    private static partial void LogTaskCreationFailed(ILogger logger, string title, string reason);
}
