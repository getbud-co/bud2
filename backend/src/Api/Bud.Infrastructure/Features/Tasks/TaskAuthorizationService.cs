using Bud.Application.Common;
using Bud.Application.Features.Tasks;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;

namespace Bud.Infrastructure.Features.Tasks;

public sealed class TaskAuthorizationService(
    ITaskRepository taskRepository,
    ITenantProvider tenantProvider) : IReadAuthorizationRule<TaskResource>, IWriteAuthorizationRule<TaskResource>, IWriteAuthorizationRule<CreateTaskContext>
{
    public async Task<Result> EvaluateAsync(TaskResource resource, CancellationToken cancellationToken = default)
        => await TenantScopedAuthorization.AuthorizeReadAsync(
            tenantProvider,
            ct => taskRepository.GetByIdAsync(resource.TaskId, ct),
            task => task.OrganizationId,
            "Tarefa não encontrada.",
            "Você não tem permissão para acessar esta tarefa.",
            cancellationToken);

    async Task<Result> IWriteAuthorizationRule<TaskResource>.EvaluateAsync(TaskResource resource, CancellationToken cancellationToken)
        => await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            ct => taskRepository.GetByIdAsync(resource.TaskId, ct),
            task => task.OrganizationId,
            "Tarefa não encontrada.",
            "Colaborador não identificado.",
            "Você não tem permissão para atualizar esta tarefa.",
            cancellationToken);

    async Task<Result> IWriteAuthorizationRule<CreateTaskContext>.EvaluateAsync(CreateTaskContext context, CancellationToken cancellationToken)
    {
        var mission = await taskRepository.GetMissionByIdAsync(context.MissionId, cancellationToken);
        if (mission is null)
        {
            return Result.NotFound("Meta não encontrada.");
        }

        return await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            mission.OrganizationId,
            "Funcionário não identificado.",
            "Você não tem permissão para criar tarefas nesta meta.");
    }
}
