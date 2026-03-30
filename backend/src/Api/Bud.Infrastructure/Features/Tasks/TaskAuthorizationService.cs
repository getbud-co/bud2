using Bud.Application.Common;
using Bud.Application.Features.Tasks;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;

namespace Bud.Infrastructure.Features.Tasks;

public sealed class TaskAuthorizationService(
    ITaskRepository taskRepository,
    ITenantProvider tenantProvider) : IWriteAuthorizationRule<TaskResource>
{
    public async Task<Result> EvaluateAsync(TaskResource resource, CancellationToken cancellationToken = default)
        => await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            ct => taskRepository.GetByIdAsync(resource.TaskId, ct),
            task => task.OrganizationId,
            "Tarefa não encontrada.",
            "Colaborador não identificado.",
            "Você não tem permissão para atualizar esta tarefa.",
            cancellationToken);
}
