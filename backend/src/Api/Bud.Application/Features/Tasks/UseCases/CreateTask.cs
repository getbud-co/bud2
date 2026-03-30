using System.Security.Claims;
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
    IUnitOfWork? unitOfWork = null,
    IApplicationAuthorizationGateway? authorizationGateway = null)
{
    public Task<Result<MissionTask>> ExecuteAsync(
        ClaimsPrincipal user,
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsyncInternal(user, command, cancellationToken);

    public async Task<Result<MissionTask>> ExecuteAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
        => await ExecuteAsyncInternal(new ClaimsPrincipal(new ClaimsIdentity()), command, cancellationToken);

    private async Task<Result<MissionTask>> ExecuteAsyncInternal(
        ClaimsPrincipal user,
        CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        LogCreatingTask(logger, command.Name, command.MissionId);

        var mission = await taskRepository.GetMissionByIdAsync(command.MissionId, cancellationToken);
        if (mission is null)
        {
            LogTaskCreationFailed(logger, command.Name, "Mission not found");
            return Result<MissionTask>.NotFound(UserErrorMessages.MissionNotFound);
        }

        if (authorizationGateway is not null)
        {
            var canWrite = await authorizationGateway.CanWriteAsync(user, new MissionResource(mission.Id), cancellationToken);
            if (!canWrite)
            {
                LogTaskCreationFailed(logger, command.Name, UserErrorMessages.TaskCreateForbidden);
                return Result<MissionTask>.Forbidden(UserErrorMessages.TaskCreateForbidden);
            }
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
