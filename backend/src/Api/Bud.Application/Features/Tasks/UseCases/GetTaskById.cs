using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Tasks.UseCases;

public sealed class GetTaskById(ITaskRepository taskRepository)
{
    public async Task<Result<MissionTask>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        return task is null
            ? Result<MissionTask>.NotFound(UserErrorMessages.TaskNotFound)
            : Result<MissionTask>.Success(task);
    }
}
