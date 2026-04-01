using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Tasks.UseCases;

public sealed class GetTaskById(
    ITaskRepository taskRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<MissionTask>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return Result<MissionTask>.NotFound(UserErrorMessages.TaskNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new TaskResource(id), cancellationToken);
        if (!canRead)
        {
            return Result<MissionTask>.Forbidden(UserErrorMessages.TaskNotFound);
        }

        return Result<MissionTask>.Success(task);
    }
}
