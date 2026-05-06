using Bud.Api.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bud.Api.Features.Tasks;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.TenantSelected)]
[Route("api")]
[Produces("application/json")]
public sealed class TasksController(
    CreateTask createTask,
    GetTaskById getTaskById,
    PatchTask patchTask,
    DeleteTask deleteTask,
    IValidator<CreateTaskRequest> createValidator,
    IValidator<PatchTaskRequest> updateValidator) : ApiControllerBase
{
    /// <summary>
    /// Cria uma nova tarefa para uma meta.
    /// </summary>
    /// <response code="201">Tarefa criada com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    /// <response code="404">Meta não encontrada.</response>
    /// <response code="403">Sem permissão para criar tarefa.</response>
    [HttpPost("missions/{missionId:guid}/tasks")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskResponse>> Create(Guid missionId, CreateTaskRequest request, CancellationToken cancellationToken)
    {
        request.MissionId = missionId;

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new CreateTaskCommand(
            missionId,
            request.EmployeeId,
            request.Title,
            request.Description,
            request.SortOrder,
            request.DueDate);

        var result = await createTask.ExecuteAsync(command, cancellationToken);
        return FromResult<MissionTask, TaskResponse>(result, task =>
            CreatedAtAction(nameof(GetById), new { id = task.Id }, task.ToResponse()));
    }

    /// <summary>
    /// Busca uma tarefa pelo identificador.
    /// </summary>
    /// <response code="200">Tarefa encontrada.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpGet("tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getTaskById.ExecuteAsync(id, cancellationToken);
        return FromResultOk(result, task => task.ToResponse());
    }

    /// <summary>
    /// Atualiza uma tarefa existente.
    /// </summary>
    /// <response code="200">Tarefa atualizada com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    /// <response code="403">Sem permissão para atualizar tarefa.</response>
    [HttpPatch("tasks/{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, PatchTaskRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new PatchTaskCommand(
            request.Title,
            request.Description,
            request.IsDone,
            request.DueDate);

        var result = await patchTask.ExecuteAsync(id, command, cancellationToken);
        return FromResultOk(result, task => task.ToResponse());
    }

    /// <summary>
    /// Remove uma tarefa.
    /// </summary>
    /// <response code="204">Tarefa removida com sucesso.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    /// <response code="403">Sem permissão para excluir tarefa.</response>
    [HttpDelete("tasks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await deleteTask.ExecuteAsync(id, cancellationToken);
        return FromResult(result, NoContent);
    }
}
